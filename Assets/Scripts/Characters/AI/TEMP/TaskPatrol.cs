using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskPatrol : Node {
    Transform _transform;
    Transform[] _waypoints; //burgers
    Kim _kim;

    public TaskPatrol(Transform transform, Transform[] waypoints, Kim kim) {
        _transform = transform;
        _waypoints = waypoints;
        _kim = kim;
    }
    public override NodeState Evaluate() {
        _kim.UpdateCharacter();

        state = NodeState.RUNNING;
        return state;
    }
}
