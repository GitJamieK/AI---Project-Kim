using System.Collections.Generic;

namespace BehaviorTree {
    public class Sequence : Node {
        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate() {
            bool anyChildrenRunning = false;

            foreach (Node node in children) {
                switch (node.Evaluate()) {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        anyChildrenRunning = true;
                        continue;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            state = anyChildrenRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}