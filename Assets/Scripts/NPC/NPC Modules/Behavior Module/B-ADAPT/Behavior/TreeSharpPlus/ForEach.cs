using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreeSharpPlus
{
    public enum StatusChange { Added, Removed };

    /// <summary>
    /// Handler class for events revolving around adding or removing
    /// participants in a ForEach node.
    /// </summary>
    public class ForEachEventArgs<T> : EventArgs
    {
        private readonly StatusChange statusChange;
        private readonly T participant;

        public StatusChange StatusChange { get { return this.statusChange; } }
        public T Participant { get { return this.participant; } }

        public ForEachEventArgs(StatusChange statusChange, T participant)
        {
            this.statusChange = statusChange;
            this.participant = participant;
        }
    }

    /// <summary>
    /// Executes a subtree across a mutable group of participants. If a
    /// participant is added, a new subtree will be automatically generated
    /// for that participant to execute. Likewise, if a participant is removed,
    /// its corresponding tree will be terminated (without causing failure in
    /// the ForEach node). If any subtree of the ForEach node fails naturally,
    /// this node returns Failure. ForEach returns success once all subtrees
    /// terminate.
    /// </summary>
    /// <typeparam name="T">ForEach's participant type.</typeparam>
    public class ForEach<T> : Parallel
    {
        // Delegate type for participant add/removal notifications
        public delegate void ParticipantEventHandler(
            object sender,
            ForEachEventArgs<T> e);

        public event EventHandler<ForEachEventArgs<T>> ParticipantsChanged;

        // The mutable participant list
        private Val<IEnumerable<T>> participants;

        // Mapping of child subtrees to their participant owners
        private Dictionary<T, Node> participantToChild;

        // Subtree factory (takes an participant and produces a subtree for it)
        private Func<T, Node> childFactory;

        // Status of each child subtree
        private List<RunStatus> childStatus;

        private HashSet<Node> childrenToTerminate;

        /// <summary>
        /// Constructs a ForEach node that executes a particular subtree on a
        /// mutable set of participants
        /// </summary>
        /// <param name="subtreeFactory">Factory function for creating a
        /// subtree for each participant</param>
        /// <param name="participants">Participating objects can be safely
        /// modified during execution of this tree</param>
        public ForEach(Func<T, Node> subtreeFactory, IEnumerable<T> participants)
        {
            this.participantToChild = new Dictionary<T, Node>();
            this.childStatus = new List<RunStatus>();
            this.childrenToTerminate = new HashSet<Node>();
            //this.participants = participants;

            // Generate subtrees for each of the current children
            foreach (T obj in participants)
            {
                this.participantToChild.Add(obj, subtreeFactory.Invoke(obj));
                this.childStatus.Add(RunStatus.Running);
            }
            this.childFactory = subtreeFactory;

            this.Children = new List<Node>(participantToChild.Values);

            foreach (Node node in Children)
                node.Parent = this;
        }

        /// <summary>
        /// Lazy constructor for an immutable list of objects with the
        /// params keyword (for easy initialization)
        /// </summary>
        /// <param name="subtreeFactory">Factory function for creating a
        /// subtree for each participant</param>
        /// <param name="participants">Immutable array of participants</param>
        //public ForEach(Func<T, Node> subtreeFactory, params T[] participants)
        //    : this(subtreeFactory, new Val<IEnumerable<T>>(participants)) { }

        protected RunStatus TerminateChildren()
        {
            return TreeUtils.DoUntilComplete<Node>(
                (Node n) => n.Terminate(),
                this.Children);
        }

        public void AddParticipant(T participant)
        {
            Node newChild = this.childFactory.Invoke(participant);
            this.participantToChild[participant] = newChild;
            this.Children.Add(newChild);

            newChild.Start();
            this.childStatus.Add(RunStatus.Running);
            this.runningChildren++;
        }

        public RunStatus RemoveParticipant(T participant)
        {
            Node child = this.participantToChild[participant];
            int index = this.Children.IndexOf(child);
            if (index >= 0)
            {
                this.Children.RemoveAt(index);

                if (this.childStatus[index] == RunStatus.Running)
                    this.runningChildren--;

                this.childStatus.RemoveAt(index);
                if (this.IsRunning)
                {
                    childrenToTerminate.Add(child);
                }
            }
            if (this.IsRunning)
            {
                Debug.Log("Terminating: " + participantToChild[participant].LastTerminationStatus.Value.ToString());
                return participantToChild[participant].LastTerminationStatus.Value;
            }
            else
            {
                return RunStatus.Success;
            }
            //participantToChild.Remove(participant);
        }

        private RunStatus UpdateChildren()
        {
            RunStatus finalResult = RunStatus.Success;

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.childStatus[i] == RunStatus.Running)
                {
                    Node node = this.Children[i];
                    RunStatus tickResult = node.Tick();

                    // Check to see if anything finished
                    if (tickResult == RunStatus.Running)
                    {
                        finalResult = RunStatus.Running;
                    }
                    else
                    {
                        // Clean up the node
                        node.Stop();
                        this.childStatus[i] = tickResult;
                        this.runningChildren--;

                        // If the node failed
                        if (tickResult == RunStatus.Failure)
                        {
                            // Add everything to the termination bucket
                            foreach (Node child in this.Children)
                                this.childrenToTerminate.Add(child);

                            // Report failure
                            return RunStatus.Failure;
                        }
                    }
                }
            }

            return finalResult;
        }

        public override IEnumerable<RunStatus> Execute()
        {
            RunStatus executionStatus = RunStatus.Running;

            while (true)
            {
                // Skip if the child nodes have already been resolved.
                // Note that this will ignore the addition/removal of
                // children after the current ones have finished.
                if (executionStatus == RunStatus.Running)
                {
                    executionStatus = this.UpdateChildren();
                }

                // Make sure all of the children have terminated even
                // if we're finished everything else.
                if (this.childrenToTerminate.Count > 0)
                {
                    List<Node> termList =
                        new List<Node>(this.childrenToTerminate);
                    foreach (Node node in termList)
                    {
                        if (node.Terminate() != RunStatus.Running)
                            this.childrenToTerminate.Remove(node);
                    }

                    // This will result in one extra tick
                    yield return RunStatus.Running;
                }
                // If all of the children have finished and everything
                // waiting to terminate has done so, we're good.
                else if (executionStatus != RunStatus.Running)
                {
                    Debug.Log("ForEach node has terminated with " + executionStatus.ToString());
                    yield return executionStatus;
                    yield break;
                }

                yield return RunStatus.Running;
            }
        }
    }
}

//NOTE: I took the ForEach node from the old project, which should work as far as I know. I just left the newer ForEach code commented out so it could easily be merged
//when necessary.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace TreeSharpPlus
//{
//    public enum StatusChange { Added, Removed };

//    //
//    // TODO: Disabled this for now because of some type issues. - AS
//    //
//    /// <summary>
//    /// Handler class for events revolving around adding or removing
//    /// participants in a ForEach node.
//    /// </summary>
//    //public class ForEachEventArgs<T> : EventArgs where T : IHasBehaviorObject
//    //{
//    //    private readonly StatusChange statusChange;
//    //    private readonly T participant;

//    //    public StatusChange StatusChange { get { return this.statusChange; } }
//    //    public T Participant { get { return this.participant; } }

//    //    public ForEachEventArgs(StatusChange statusChange, T participant)
//    //    {
//    //        this.statusChange = statusChange;
//    //        this.participant = participant;
//    //    }
//    //}

//    //// Delegate type for participant add/removal notifications
//    //public delegate void ParticipantEventHandler(
//    //    object sender,
//    //    ForEachEventArgs<T> e);

//    /// <summary>
//    /// Executes a subtree across a mutable group of participants. If a
//    /// participant is added, a new subtree will be automatically generated
//    /// for that participant to execute. Likewise, if a participant is removed,
//    /// its corresponding tree will be terminated (without causing failure in
//    /// the ForEach node). If any subtree of the ForEach node fails naturally,
//    /// this node returns Failure. ForEach returns success once all subtrees
//    /// terminate.
//    /// </summary>
//    /// <typeparam name="T">ForEach's participant type.</typeparam>
//    public class ForEach<T> : Node, ISurrenderable
//        where T : class, IHasBehaviorObject
//    {
//        // Subtree factory (takes an participant and produces a subtree for it)
//        private Func<T, Node> childFactory;

//        // Mapping of child subtrees to their participant owners
//        private Dictionary<BehaviorObject, Node> activeChildren;

//        // Dangling subtrees corresponding to removed participants that we
//        // need to terminate
//        private Dictionary<BehaviorObject, Node> terminatingChildren;

//        /// <summary>
//        /// Constructs a ForEach node that executes a particular subtree on a
//        /// mutable set of participants
//        /// </summary>
//        /// <param name="subtreeFactory">Factory function for creating a
//        /// subtree for each participant</param>
//        /// <param name="participants">Participating objects can be safely
//        /// modified during execution of this tree</param>
//        public ForEach(Func<T, Node> subtreeFactory, IEnumerable<T> participants)
//            : base()
//        {
//            this.childFactory = subtreeFactory;
//            this.activeChildren = new Dictionary<BehaviorObject, Node>();
//            this.terminatingChildren = new Dictionary<BehaviorObject, Node>();

//            // Generate subtrees for each of the current children
//            foreach (T obj in participants)
//            {
//                Node newChild = subtreeFactory(obj);
//                this.activeChildren.Add(obj.Behavior, newChild);
//                newChild.Parent = this;
//            }
//        }

//        public void AddParticipant(T participant)
//        {
//            BehaviorObject obj = participant.Behavior;

//        }

//        public void AddParticipant(T participant)
//        {
//            Node newChild = this.childFactory.Invoke(participant);
//            this.participantToChild[participant] = newChild;
//            this.Children.Add(newChild);

//            newChild.Start();
//            this.childStatus.Add(RunStatus.Running);
//            this.runningChildren++;
//        }

//        public void RemoveParticipant(T participant)
//        {
//            Node child = this.participantToChild[participant];
//            int index = this.Children.IndexOf(child);
//            this.Children.RemoveAt(index);

//            if (this.childStatus[index] == RunStatus.Running)
//                this.runningChildren--;

//            this.childStatus.RemoveAt(index);
//            childrenToTerminate.Add(child);
//            //participantToChild.Remove(participant);
//        }

//        /// <summary>
//        /// Surrender a participant based on its contained BehaviorObject
//        /// </summary>
//        public void Surrender(BehaviorObject obj)
//        {
//            // Search for the BehaviorObject
//            BehaviorObject found = null;

//            // Are we already terminating it?
//            foreach (T participant in this.childrenToTerminate

//            T found = null;
//            foreach (T participant in this.participants.Value)
//                if (participant.Behavior == obj)
//                    found = participant;
//            if (found != null)
//                this.RemoveParticipant(found);
//        }

//        public RunStatus TerminationStatus(T participant)
//        {
//            return participantToChild[participant].LastTerminationStatus.Value;
//        }

//        private RunStatus UpdateChildren()
//        {
//            RunStatus finalResult = RunStatus.Success;

//            for (int i = 0; i < this.Children.Count; i++)
//            {
//                if (this.childStatus[i] == RunStatus.Running)
//                {
//                    Node node = this.Children[i];
//                    RunStatus tickResult = node.Tick();

//                    // Check to see if anything finished
//                    if (tickResult == RunStatus.Running)
//                    {
//                        finalResult = RunStatus.Running;
//                    }
//                    else
//                    {
//                        // Clean up the node
//                        node.Stop();
//                        this.childStatus[i] = tickResult;
//                        this.runningChildren--;

//                        // If the node failed
//                        if (tickResult == RunStatus.Failure)
//                        {
//                            // Add everything to the termination bucket
//                            foreach (Node child in this.Children)
//                                this.childrenToTerminate.Add(child);

//                            // Report failure
//                            return RunStatus.Failure;
//                        }
//                    }
//                }
//            }

//            return finalResult;
//        }

//        public override IEnumerable<RunStatus> Execute()
//        {
//            RunStatus executionStatus = RunStatus.Running;

//            while (true)
//            {
//                // Skip if the child nodes have already been resolved.
//                // Note that this will ignore the addition/removal of
//                // children after the current ones have finished.
//                if (executionStatus == RunStatus.Running)
//                {
//                    this.UpdateParticipants();
//                    executionStatus = this.UpdateChildren();
//                }

//                // Make sure all of the children have terminated even
//                // if we're finished everything else.
//                if (this.childrenToTerminate.Count > 0)
//                {
//                    List<Node> termList =
//                        new List<Node>(this.childrenToTerminate);
//                    foreach (Node node in termList)
//                    {
//                        if (node.Terminate() != RunStatus.Running)
//                            this.childrenToTerminate.Remove(node);
//                    }

//                    // This will result in one extra tick
//                    yield return RunStatus.Running;
//                }
//                // If all of the children have finished and everything
//                // waiting to terminate has done so, we're good.
//                else if (executionStatus != RunStatus.Running)
//                {
//                    Debug.Log("ForEach node has terminated with " + executionStatus.ToString());
//                    yield return executionStatus;
//                    yield break;
//                }

//                yield return RunStatus.Running;
//            }
//        }
//    }
//}
