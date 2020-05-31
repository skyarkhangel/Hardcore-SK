using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    /// <summary>
    /// For testing messages if something breaks.
    /// </summary>
    public class TransitionAction_MessageDebug : TransitionAction
    {
        public TargetInfo lookTarget = TargetInfo.Invalid;
        public Func<TargetInfo> lookTargetGetter;
        public string message;
        public float repeatAvoiderSeconds;
        public string repeatAvoiderTag;
        public MessageTypeDef type;

        public TransitionAction_MessageDebug(string message, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f) : this(message, MessageTypeDefOf.NeutralEvent, repeatAvoiderTag, repeatAvoiderSeconds) { }

        public TransitionAction_MessageDebug(string message, MessageTypeDef messageType, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
        {
            this.message = message;
            type = messageType;
            this.repeatAvoiderTag = repeatAvoiderTag;
            this.repeatAvoiderSeconds = repeatAvoiderSeconds;
        }

        public TransitionAction_MessageDebug(string message, MessageTypeDef messageType, TargetInfo lookTarget, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
        {
            this.message = message;
            type = messageType;
            this.lookTarget = lookTarget;
            this.repeatAvoiderTag = repeatAvoiderTag;
            this.repeatAvoiderSeconds = repeatAvoiderSeconds;
        }

        public TransitionAction_MessageDebug(string message, MessageTypeDef messageType, Func<TargetInfo> lookTargetGetter, string repeatAvoiderTag = null, float repeatAvoiderSeconds = 1f)
        {
            this.message = message;
            type = messageType;
            this.lookTargetGetter = lookTargetGetter;
            this.repeatAvoiderTag = repeatAvoiderTag;
            this.repeatAvoiderSeconds = repeatAvoiderSeconds;
        }

        public override void DoAction(Transition trans)
        {
            try
            {
                if (!repeatAvoiderTag.NullOrEmpty() && !MessagesRepeatAvoider.MessageShowAllowed(repeatAvoiderTag, repeatAvoiderSeconds)) return;
                var targetInfo = lookTargetGetter?.Invoke() ?? lookTarget;
                if (!targetInfo.IsValid)
                {
                    if(trans == null) Log.Message($"trans == null");
                    if(trans?.target == null) Log.Message($"target == null");
                    if(trans?.target?.lord == null) Log.Message($"lord == null");
                    if(trans?.target?.lord?.ownedPawns == null) Log.Message($"ownedPawns == null");
                    targetInfo = trans.target.lord.ownedPawns.FirstOrDefault();
                }
                Messages.Message(message, targetInfo, type);
            }
            catch (Exception e)
            {
                Log.Error($"Exception: {e}");
                Log.Error($"Message failed ({message})");
            }
        }
    }
}
