using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using Steamworks;
using UnityEngine;
using static RocketMan.Grapher;

namespace RocketMan
{
    public class GraphPointCollection
    {
        private float timeWindow = 250;

        private readonly List<GraphPoint> points = new List<GraphPoint>();

        public bool Ready
        {
            get => Count > 16;
        }

        public int Count
        {
            get => points.Count;
        }

        public float TargetTimeWindowSize
        {
            get => timeWindow;
            set
            {
                if (timeWindow != value)
                {
                    timeWindow = value;
                    Rebuild();
                }
            }
        }

        private float _minY = float.MaxValue;

        public float MinY
        {
            get => _minY;
        }

        private float _maxY = float.MinValue;

        public float MaxY
        {
            get => _maxY;
        }

        public float MinT
        {
            get => First.t;
        }

        public float MaxT
        {
            get => Last.t;
        }

        public float RangeT
        {
            get => Mathf.Min(Last.t - First.t, timeWindow);
        }

        public float RangeY
        {
            get => MaxY - MinY;
        }

        public GraphPoint First
        {
            get => points.First();
        }

        public GraphPoint Last
        {
            get => points.Last();
        }

        public IEnumerable<GraphPoint> Points
        {
            get => points;
        }

        public GraphPointCollection()
        {
        }

        private int _maxAge = 0;
        private int _minAge = 0;
        private int _streak = 0;

        public void Add(GraphPoint point)
        {
            if (Count < 16)
            {
                Commit(point);
                return;
            }
            if (points.Count >= 1500)
            {
                points.RemoveAt(0);
            }
            if (Last.t == point.t)
            {
                point.y += Last.y;
                if (point.y > _maxY)
                {
                    _maxAge = Mathf.Min(15, points.Count);
                    _maxY = point.y;
                }
                if (point.y < _minY)
                {
                    _minAge = Mathf.Min(15, points.Count);
                    _minY = point.y;
                }
                points[points.Count - 1] = point;
                return;
            }

            GraphPoint pNm1 = Last;
            GraphPoint pNm2 = points[points.Count - 2];

            if (pNm1.t == pNm2.t)
            {
                Commit(point);
                return;
            }
            float m1 = (pNm1.y - pNm2.y) / (pNm1.t - pNm2.t);
            float m0 = (point.y - pNm1.y) / (point.t - pNm1.t);

            if (Mathf.Abs(m1 - m0) < 1e-3)
            {
                if (_streak++ > 1 && point.color == pNm1.color)
                {
                    points[points.Count - 1] = point;
                    return;
                }
                Commit(point);
                return;
            }
            _streak = 0;

            Commit(point);
        }

        public void Rebuild()
        {
            if (Count < 3) return;

            int position = 0;

            while (position < points.Count - 3 && Last.t - points[position].t > timeWindow)
                position++;

            if (position > 0 && position < points.Count)
            {
                GraphPoint p0 = points[position - 1];
                GraphPoint p1 = points[position];

                if (p0.t != p1.t)
                {
                    float t1 = Last.t - timeWindow;
                    float m = (p1.y - p0.y) / (p1.t - p0.t);

                    p0.y = m * (t1 - p0.t) + p0.y;
                    p0.t = t1;

                    points[position - 1] = p0;
                }
                position -= 2;

                while (position >= 0)
                {
                    points.RemoveAt(position);
                    position--;
                }
            }

            if ((_maxAge > 0 && _minAge > 0))
            {
                _maxAge = Math.Max(_maxAge - 1, 0);
                _minAge = Math.Max(_minAge - 1, 0);
            }
            else
            {
                UpdateCriticalPoints();
            }
        }

        private void Commit(GraphPoint point)
        {
            points.Add(point);
            if (point.y > _maxY)
            {
                _maxAge = Mathf.Min(15, points.Count);
                _maxY = point.y;
            }
            if (point.y < _minY)
            {
                _minAge = Mathf.Min(15, points.Count);
                _minY = point.y;
            }
        }

        private void UpdateCriticalPoints()
        {
            GraphPoint last = Last;

            _minY = last.y;
            _maxY = last.y;

            for (int i = 0; i < Count; i++)
            {
                GraphPoint point = points[i];
                if (_minY > point.y)
                {
                    _minAge = Math.Min(i, 15);
                    _minY = point.y;
                }
                if (_maxY < point.y)
                {
                    _maxAge = Math.Min(i, 15);
                    _maxY = point.y;
                }
            }
        }
    }
}
