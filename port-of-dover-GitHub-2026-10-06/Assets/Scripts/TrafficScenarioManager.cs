using System;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficScenarioType { FreeFlow, Congestion }
public enum TrafficScenarioState { Idle, Running, Paused, Completed }

/// <summary>Development/test scenarios only; not a calibrated port traffic model.</summary>
public class TrafficScenarioManager : MonoBehaviour
{
    [Serializable]
    private class Parameters
    {
        [Min(1)] public int vehicleCount;
        [Min(0.01f)] public float normalSpeed;
        [Min(0.01f)] public float spawnInterval;
        public Parameters(int count, float speed, float interval)
        { vehicleCount = count; normalSpeed = speed; spawnInterval = interval; }
    }

    [Header("Scenario — changes apply after Reset / Start")]
    [SerializeField] private TrafficScenarioType scenario = TrafficScenarioType.FreeFlow;
    [Header("Vehicle — explicit scene references")]
    [SerializeField] private GameObject vehiclePrefab;
    [SerializeField] private Transform route;
    [SerializeField] private Transform vehiclesParent;
    [SerializeField] private GameObject singleVehicleDebug;
    [Header("Free Flow — Development / Test Parameters")]
    [SerializeField] private Parameters freeFlow = new Parameters(5, 10, 8);
    [Header("Congestion — Development / Test Parameters")]
    [SerializeField] private Parameters congestion = new Parameters(10, 10, 4);
    [SerializeField, Min(0.01f)] private float queueSpeed = 2.5f;
    [SerializeField, Min(0)] private float processingWaitingTime = 12f;
    [Header("Queue — route-center spacing, metres")]
    [SerializeField, Min(12)] private float minimumCenterSpacing = 16f;

    private class Vehicle
    {
        public VehicleRouteFollower follower;
        public bool processed;
        public float serviceElapsed;
    }
    private readonly List<Vehicle> vehicles = new List<Vehicle>();
    private Transform ownedContainer;
    private GameObject lockedPrefab;
    private Transform lockedRoute;
    private int lockedCount;
    private float lockedNormalSpeed, lockedSpawnInterval, lockedQueueSpeed, lockedWaitingTime, lockedSpacing;
    private float queueStartProgress, processingProgress, spawnRemaining;
    private int vehicleLayer;

    public TrafficScenarioState State { get; private set; } = TrafficScenarioState.Idle;
    public TrafficScenarioType CurrentScenario { get; private set; }
    public int SpawnedVehicleCount { get; private set; }
    public int ActiveVehicleCount => vehicles.Count;
    public int CompletedVehicleCount { get; private set; }
    public float ElapsedTime { get; private set; }

    [ContextMenu("Start Scenario")]
    public void StartScenario()
    {
        if (!Application.isPlaying) return;
        if (State == TrafficScenarioState.Paused) { State = TrafficScenarioState.Running; return; }
        if (State != TrafficScenarioState.Idle) return;
        Parameters selected = scenario == TrafficScenarioType.FreeFlow ? freeFlow : congestion;
        vehicleLayer = LayerMask.NameToLayer("Vehicle");
        if (!ValidateConfiguration(selected)) return;
        CurrentScenario = scenario;
        lockedCount = selected.vehicleCount;
        lockedNormalSpeed = selected.normalSpeed;
        lockedSpawnInterval = selected.spawnInterval;
        lockedQueueSpeed = queueSpeed;
        lockedWaitingTime = processingWaitingTime;
        lockedSpacing = minimumCenterSpacing;
        lockedPrefab = vehiclePrefab;
        lockedRoute = route;
        float distance = 0;
        for (int i = 1; i < lockedRoute.childCount; i++)
        {
            distance += Vector3.ProjectOnPlane(lockedRoute.GetChild(i).position - lockedRoute.GetChild(i - 1).position, Vector3.up).magnitude;
            if (i == 9) queueStartProgress = distance;
            if (i == 12) processingProgress = distance;
        }
        if (singleVehicleDebug) singleVehicleDebug.SetActive(false);
        ownedContainer = new GameObject("ScenarioVehicles").transform;
        ownedContainer.SetParent(vehiclesParent, false);
        State = TrafficScenarioState.Running;
        SpawnVehicle();
        spawnRemaining = lockedSpawnInterval;
    }

    [ContextMenu("Stop Scenario")]
    public void StopScenario()
    {
        if (State == TrafficScenarioState.Running) State = TrafficScenarioState.Paused;
    }

    [ContextMenu("Reset Scenario")]
    public void ResetScenario()
    {
        if (!Application.isPlaying) return;
        // Deactivate immediately; Unity destruction completes at end of frame.
        if (ownedContainer) { ownedContainer.gameObject.SetActive(false); Destroy(ownedContainer.gameObject); }
        ownedContainer = null;
        vehicles.Clear();
        SpawnedVehicleCount = CompletedVehicleCount = 0;
        ElapsedTime = spawnRemaining = 0;
        CurrentScenario = default;
        State = TrafficScenarioState.Idle;
    }

    private bool ValidateConfiguration(Parameters p)
    {
        bool valid = vehiclePrefab && route && vehiclesParent && singleVehicleDebug && vehicleLayer >= 8
            && route.childCount >= 15 && p != null && p.vehicleCount > 0
            && Positive(p.normalSpeed) && Positive(p.spawnInterval) && Positive(queueSpeed)
            && Finite(processingWaitingTime) && processingWaitingTime >= 0
            && Finite(minimumCenterSpacing) && minimumCenterSpacing >= 12f;
        if (valid)
            for (int i = 1; i < route.childCount; i++)
                if (Vector3.ProjectOnPlane(route.GetChild(i).position - route.GetChild(i - 1).position, Vector3.up).sqrMagnitude < 0.0001f)
                    valid = false;
        if (!valid) Debug.LogError("Scenario not started: assign all scene references, Vehicle layer and finite positive test parameters; route needs 15 ordered, distinct horizontal waypoints and spacing >= 12 m.", this);
        return valid;
    }
    private static bool Finite(float n) => !float.IsNaN(n) && !float.IsInfinity(n);
    private static bool Positive(float n) => Finite(n) && n > 0;

    private void SpawnVehicle()
    {
        GameObject instance = Instantiate(lockedPrefab, ownedContainer);
        instance.name = "TestTruck_" + SpawnedVehicleCount.ToString("D2");
        foreach (Transform child in instance.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = vehicleLayer;
        var follower = instance.GetComponent<VehicleRouteFollower>();
        if (!follower) follower = instance.AddComponent<VehicleRouteFollower>();
        follower.Initialize(lockedRoute, lockedNormalSpeed, ~(1 << vehicleLayer));
        follower.StartMovement();
        vehicles.Add(new Vehicle { follower = follower });
        SpawnedVehicleCount++;
    }

    private void Update()
    {
        if (State != TrafficScenarioState.Running) return;
        float dt = Time.deltaTime;
        ElapsedTime += dt;
        float leaderProgress = float.PositiveInfinity;
        bool serviceHeadAssigned = false;
        // Creation order is route order: predecessor moves first; followers use its updated progress.
        for (int i = 0; i < vehicles.Count;)
        {
            Vehicle car = vehicles[i];
            var follower = car.follower;
            float progress = follower.RouteProgress;
            float allowed = Mathf.Max(0, leaderProgress - lockedSpacing - progress);
            float speed = lockedNormalSpeed;
            if (CurrentScenario == TrafficScenarioType.Congestion)
            {
                if (progress < queueStartProgress - 0.001f)
                    allowed = Mathf.Min(allowed, queueStartProgress - progress);
                else if (!car.processed) speed = lockedQueueSpeed;
                if (!car.processed)
                {
                    bool head = !serviceHeadAssigned;
                    serviceHeadAssigned = true;
                    if (head && progress >= processingProgress - 0.001f)
                    {
                        // Count only time already spent at the processing point, not approach time.
                        car.serviceElapsed += dt;
                        if (car.serviceElapsed >= lockedWaitingTime && allowed > 0.001f) car.processed = true;
                        else allowed = 0;
                    }
                    if (!car.processed) allowed = Mathf.Min(allowed, Mathf.Max(0, processingProgress - progress));
                    else speed = lockedNormalSpeed;
                }
            }
            follower.Speed = speed;
            follower.TickMovement(dt, allowed);
            if (follower.IsFinished)
            {
                follower.gameObject.SetActive(false);
                Destroy(follower.gameObject);
                vehicles.RemoveAt(i);
                CompletedVehicleCount++;
                continue;
            }
            leaderProgress = follower.RouteProgress;
            i++;
        }
        if (SpawnedVehicleCount < lockedCount)
        {
            spawnRemaining = Mathf.Max(0, spawnRemaining - dt);
            bool entranceClear = vehicles.Count == 0 || vehicles[vehicles.Count - 1].follower.RouteProgress >= lockedSpacing;
            if (spawnRemaining <= 0 && entranceClear) { SpawnVehicle(); spawnRemaining = lockedSpawnInterval; }
        }
        if (SpawnedVehicleCount == lockedCount && vehicles.Count == 0) State = TrafficScenarioState.Completed;
    }

    private void OnDisable() => StopScenario();
    private void OnDestroy()
    {
        if (ownedContainer) { ownedContainer.gameObject.SetActive(false); Destroy(ownedContainer.gameObject); }
    }
}
