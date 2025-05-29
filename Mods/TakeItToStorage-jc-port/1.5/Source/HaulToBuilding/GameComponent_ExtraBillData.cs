using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HaulToBuilding;

public class GameComponent_ExtraBillData : GameComponent
{
    public static GameComponent_ExtraBillData Instance;
    private Dictionary<Bill, ExtraBillData> data = new();
    private List<ExtraBillData> datas;
    private List<Bill> keys;

    public GameComponent_ExtraBillData(Game game) => Instance = this;

    public ExtraBillData GetData(Bill bill, bool createIfMissing = true)
    {
        if (data == null)
        {
            Log.Error("[HaulToBuilding] Extra Data is null, fixing");
            data = new Dictionary<Bill, ExtraBillData>();
        }

        if (data.TryGetValue(bill, out var extraBillData)) return extraBillData;
        if (!createIfMissing) return null;
        extraBillData = new ExtraBillData();
        data.Add(bill, extraBillData);
        return extraBillData;
    }

    public void SetData(Bill bill, ExtraBillData extraBillData)
    {
        data.SetOrAdd(bill, extraBillData);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        data.RemoveAll(kv => kv.Key?.billStack == null || kv.Key.DeletedOrDereferenced);
        Scribe_Collections.Look(ref data, "data", LookMode.Reference, LookMode.Deep, ref keys, ref datas);
    }
}

public class ExtraBillData : IExposable
{
    public Building_Storage LookInStorage;
    public bool NeedCheck;
    public Building_Storage Storage;
    public TakeFromData TakeFrom = new();

    public void ExposeData()
    {
        Scribe_References.Look(ref Storage, "storage");
        Scribe_References.Look(ref LookInStorage, "lookInStorage");
        Scribe_Deep.Look(ref TakeFrom, "takeFrom");
        Scribe_Values.Look(ref NeedCheck, "needCheck");
        TakeFrom ??= new TakeFromData();
    }

    public ExtraBillData Clone() =>
        new()
        {
            LookInStorage = LookInStorage,
            Storage = Storage,
            TakeFrom = TakeFrom.Clone()
        };

    public class TakeFromData : IExposable, IList<ISlotGroupParent>
    {
        private List<Building_Storage> buildings = new();
        private List<Zone_Stockpile> stockpiles = new();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref stockpiles, "stockpiles", LookMode.Reference);
            Scribe_Collections.Look(ref buildings, "buildings", LookMode.Reference);
        }

        public IEnumerator<ISlotGroupParent> GetEnumerator() => new SlotGroups(buildings, stockpiles);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => buildings.Count + stockpiles.Count;
        public bool IsReadOnly => false;

        public void Add(ISlotGroupParent item)
        {
            switch (item)
            {
                case null:
                    return;
                case Building_Storage building:
                    buildings.Add(building);
                    break;
                case Zone_Stockpile stockpile:
                    stockpiles.Add(stockpile);
                    break;
                default:
                    Log.ErrorOnce("Unrecognized ISlotGroupParent type: " + item.GetType(),
                        item.GetHashCode());
                    break;
            }
        }

        public void CopyTo(ISlotGroupParent[] array, int arrayIndex)
        {
            buildings.Cast<ISlotGroupParent>().ToList().CopyTo(array, arrayIndex);
            stockpiles.Cast<ISlotGroupParent>().ToList().CopyTo(array, arrayIndex + buildings.Count);
        }

        public bool Remove(ISlotGroupParent item)
        {
            switch (item)
            {
                case null: return buildings.Remove(null) || stockpiles.Remove(null);
                case Building_Storage building: return buildings.Remove(building);
                case Zone_Stockpile stockpile: return stockpiles.Remove(stockpile);
                default:
                    Log.ErrorOnce("Unrecognized ISlotGroupParent type: " + item.GetType(), item.GetHashCode());
                    return false;
            }
        }

        public bool Contains(ISlotGroupParent item)
        {
            switch (item)
            {
                case null: return buildings.Contains(null) || stockpiles.Contains(null);
                case Building_Storage building: return buildings.Contains(building);
                case Zone_Stockpile stockpile: return stockpiles.Contains(stockpile);
                default:
                    Log.ErrorOnce("Unrecognized ISlotGroupParent type: " + item.GetType(), item.GetHashCode());
                    return false;
            }
        }

        public void Clear()
        {
            buildings.Clear();
            stockpiles.Clear();
        }

        public int IndexOf(ISlotGroupParent item)
        {
            switch (item)
            {
                case null:
                    return buildings.Contains(null)
                        ? buildings.IndexOf(null)
                        : stockpiles.IndexOf(null) + buildings.Count;
                case Building_Storage building: return buildings.IndexOf(building);
                case Zone_Stockpile stockpile: return stockpiles.IndexOf(stockpile) + buildings.Count;
                default:
                    Log.ErrorOnce("Unrecognized ISlotGroupParent type: " + item.GetType(), item.GetHashCode());
                    return -1;
            }
        }

        public void Insert(int index, ISlotGroupParent item)
        {
            switch (item)
            {
                case null:
                    buildings.Insert(index, null);
                    break;
                case Building_Storage building:
                    buildings.Insert(index, building);
                    break;
                case Zone_Stockpile stockpile:
                    stockpiles.Insert(index + buildings.Count, stockpile);
                    break;
                default:
                    Log.ErrorOnce("Unrecognized ISlotGroupParent type: " + item.GetType(), item.GetHashCode());
                    break;
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= buildings.Count) stockpiles.RemoveAt(index - buildings.Count);
            else buildings.RemoveAt(index);
        }

        public ISlotGroupParent this[int index]
        {
            get => index >= buildings.Count ? stockpiles[index - buildings.Count] : buildings[index];
            set
            {
                if (index > buildings.Count) stockpiles[index - buildings.Count] = (Zone_Stockpile)value;
                else buildings[index] = (Building_Storage)value;
            }
        }

        public bool Any() => buildings.Any() || stockpiles.Any();

        public TakeFromData Clone() =>
            new()
            {
                buildings = buildings.ListFullCopy(),
                stockpiles = stockpiles.ListFullCopy()
            };

        public class SlotGroups : IEnumerator<ISlotGroupParent>
        {
            private readonly IEnumerator<Building_Storage> buildings;
            private readonly IEnumerator<Zone_Stockpile> stockpiles;
            private bool finishedBuildings;

            public SlotGroups(IEnumerable<Building_Storage> b, IEnumerable<Zone_Stockpile> z)
            {
                buildings = b.GetEnumerator();
                stockpiles = z.GetEnumerator();
            }

            public void Dispose()
            {
                buildings.Dispose();
                stockpiles.Dispose();
                finishedBuildings = false;
            }

            public bool MoveNext()
            {
                finishedBuildings = !buildings.MoveNext();
                return finishedBuildings ? stockpiles.MoveNext() : !finishedBuildings;
            }

            public void Reset()
            {
                buildings.Reset();
                stockpiles.Reset();
            }

            public ISlotGroupParent Current => finishedBuildings ? stockpiles.Current : buildings.Current;

            object IEnumerator.Current => Current;
        }
    }
}
