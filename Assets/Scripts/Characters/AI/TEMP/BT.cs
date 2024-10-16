using BehaviorTree;

public class BT : Tree {
    public UnityEngine.Transform[] waypoints;
    Kim _kim;

    void Awake() {
        _kim = GetComponent<Kim>();
    }
    protected override Node SetupTree() {
        Node root = new TaskPatrol(transform, waypoints, _kim);
        return root;
    }
}