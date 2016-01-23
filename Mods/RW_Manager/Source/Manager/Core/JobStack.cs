// Manager/JobStack.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-10-18 22:53

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FluffyManager
{
    public class JobStack : IExposable
    {
        private List<ManagerJob> _stack;

        /// <summary>
        ///     Jobstack of jobs that are available now
        /// </summary>
        public List<ManagerJob> CurStack
        {
            get { return _stack.Where( mj => mj.ShouldDoNow ).OrderBy( mj => mj.Priority ).ToList(); }
        }

        /// <summary>
        ///     Highest priority available job
        /// </summary>
        public ManagerJob NextJob => CurStack.FirstOrDefault();

        /// <summary>
        ///     Full jobstack, in order of assignment
        /// </summary>
        public JobStack()
        {
            _stack = new List<ManagerJob>();
        }

        public void ExposeData()
        {
            Scribe_Collections.LookList( ref _stack, "JobStack", LookMode.Deep );
        }

        /// <summary>
        ///     Jobs of type T in jobstack, in order of priority
        /// </summary>
        public List<T> FullStack<T>() where T : ManagerJob
        {
            return _stack.OrderBy( job => job.Priority ).OfType<T>().ToList();
        }

        /// <summary>
        ///     Jobs of type T in jobstack, in order of priority
        /// </summary>
        public List<ManagerJob> FullStack()
        {
            return _stack.OrderBy( job => job.Priority ).ToList();
        }

        /// <summary>
        ///     Call the worker for the next available job
        /// </summary>
        public bool TryDoNextJob()
        {
            ManagerJob job = NextJob;
            if ( job == null )
            {
                return false;
            }

            // update lastAction
            job.Touch();

            // perform next job if no action was taken
            if ( !job.TryDoJob() )
            {
                return TryDoNextJob();
            }
            return true;
        }

        /// <summary>
        /// Add job to the stack with bottom priority.
        /// </summary>
        /// <param name="job"></param>
        public void Add( ManagerJob job )
        {
            job.Priority = _stack.Count + 1;
            _stack.Add( job );
        }

        /// <summary>
        /// Cleanup job, delete from stack and update priorities.
        /// </summary>
        /// <param name="job"></param>
        public void Delete( ManagerJob job )
        {
            job.CleanUp();
            _stack.Remove( job );
            CleanPriorities();
        }

        /// <summary>
        /// Normalize priorities
        /// </summary>
        private void CleanPriorities()
        {
            List<ManagerJob> orderedStack = _stack.OrderBy( mj => mj.Priority ).ToList();
            for ( int i = 1; i <= _stack.Count; i++ )
            {
                orderedStack[i - 1].Priority = i;
            }
        }

        public void SwitchPriorities( ManagerJob a, ManagerJob b )
        {
            int tmp = a.Priority;
            a.Priority = b.Priority;
            b.Priority = tmp;
        }

        public void IncreasePriority( ManagerJob job )
        {
            ManagerJob jobB = _stack.OrderByDescending( mj => mj.Priority ).First( mj => mj.Priority < job.Priority );
            SwitchPriorities( job, jobB );
        }

        public void DecreasePriority( ManagerJob job )
        {
            ManagerJob jobB = _stack.OrderBy( mj => mj.Priority ).First( mj => mj.Priority > job.Priority );
            SwitchPriorities( job, jobB );
        }

        public void TopPriority( ManagerJob job )
        {
            job.Priority = - 1;
            CleanPriorities();
        }

        public void BottomPriority( ManagerJob job )
        {
            job.Priority = _stack.Count + 10;
            CleanPriorities();
        }

        public void TopPriority<T>( T job ) where T : ManagerJob
        {
            // get list of priorities for this type.
            List<T> jobsOfType = _stack.OfType<T>().OrderBy( j => j.Priority ).ToList();
            List<int> priorities = jobsOfType.Select( j => j.Priority ).ToList();

            // make sure our job is on top.
            job.Priority = - 1;

            // re-sort
            jobsOfType = jobsOfType.OrderBy( j => j.Priority ).ToList();

            // fill in priorities, making sure we don't affect other types.
            for ( int i = 0; i < jobsOfType.Count; i++ )
            {
                jobsOfType[i].Priority = priorities[i];
            }
        }

        public void BottomPriority<T>( T job ) where T : ManagerJob
        {
            // get list of priorities for this type.
            List<T> jobsOfType = _stack.OfType<T>().OrderBy( j => j.Priority ).ToList();
            List<int> priorities = jobsOfType.Select( j => j.Priority ).ToList();

            // make sure our job is on the bottom.
            job.Priority = _stack.Count + 10;

            // re-sort
            jobsOfType = jobsOfType.OrderBy( j => j.Priority ).ToList();

            // fill in priorities, making sure we don't affect other types.
            for ( int i = 0; i < jobsOfType.Count; i++ )
            {
                jobsOfType[i].Priority = priorities[i];
            }
        }

        public void DecreasePriority<T>( T job ) where T : ManagerJob
        {
            ManagerJob jobB = _stack.OfType<T>()
                                    .OrderBy( mj => mj.Priority )
                                    .First( mj => mj.Priority > job.Priority );
            SwitchPriorities( job, jobB );
        }

        public void IncreasePriority<T>( T job ) where T : ManagerJob
        {
            ManagerJob jobB =
                _stack.OfType<T>().OrderByDescending( mj => mj.Priority ).First( mj => mj.Priority < job.Priority );
            SwitchPriorities( job, jobB );
        }
    }
}