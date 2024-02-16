using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using UnityEngine;
using sl;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;

public class CalibrateRos : MonoBehaviour {


    // public float height = 0;
    private bool ReadyForCalibration = false;
    private bool calibrationFinished = false;

    private Vector3 ZedToCarOffset;
    private float ZedToCarYaw;
    public Transform car;
    // public float memory = 0.5f;
    // private Vector3 old_pos = Vector3.zero;
    // private Vector3 new_pos = Vector3.zero;
    
    public int smoothingFrames = 10;
    private Quaternion smoothedRotation;
    private Vector3 smoothedPosition;
    private Queue<Quaternion> rotations;
    private Queue<Vector3> positions;
    private Vector4 avgr;
    private Vector3 avgp;


    private Vector3 CarToXROffset;
    private float CarToXRYaw;

    [Range(-2.0F, 2.0F)] public float LiDARToCarX;
    [Range(-2.0F, 2.0F)] public float LiDARToCarY;
    [Range(-2.0F, 2.0F)] public float LiDARToCarZ;


    // Vector3 Filter(Vector3 oldPos, Vector3 newPos) {
    //     return memory * oldPos + (1 - memory) * newPos;
    // }
    
    public Transform VRWorldZero;

    public GameObject base_link;
    public Transform aligned_base_link;

    private Vector3 XRWorldZeroOffset;

    private Quaternion LiDAR_Roatation_Offset = Quaternion.identity;
    public Quaternion global_Roatation_Offset = Quaternion.identity;

    private bool _foundRosTransform;

    void AssignRosTransform() {
        base_link = GameObject.Find("map/os_lidar");
        if (base_link == null) { _foundRosTransform = false; }
        else { _foundRosTransform = true; }
    }

    // Start is called before the first frame update
    void Start() {
        try { AssignRosTransform(); }
        catch { ; }
        // ROSConnection.GetOrCreateInstance().Subscribe<PointMsg>("translation_offset", PointCallback);
        // ROSConnection.GetOrCreateInstance().Subscribe<QuaternionMsg>("rotation_offset", QuaternionCallback);
        rotations = new Queue<Quaternion>(smoothingFrames);
        positions = new Queue<Vector3>(smoothingFrames);
    }

    // Update is called once per frame
    void Update() {
        if (!_foundRosTransform) {
            try { AssignRosTransform(); }
            catch { ; }
        }
        else {
            if (!ReadyForCalibration) {
                ReadyForCalibration = true;
                transform.SetParent(aligned_base_link.transform);
                Debug.Log("Picked up pose, fixing relationship");
            }
            else {
                if (Input.GetKeyDown(KeyCode.X) && !calibrationFinished) {
                    calibrationFinished = true;
                    VRWorldZero.transform.SetParent(transform);
                    car.SetParent(aligned_base_link.transform);
                    Debug.Log("finished parenting");
                }
            }
            // new_pos = base_link.transform.position + LiDAR_Translation_Offset;
            // if (old_pos == Vector3.zero) {
            //     aligned_base_link.position = new_pos;
            //     old_pos = new_pos;
            // }
            // else {
                // old_pos = aligned_base_link.position;
                // aligned_base_link.position = Filter(old_pos, new_pos);
                // aligned_base_link.position = Vector3.Lerp(aligned_base_link.position, new_pos, Time.deltaTime * transitionSpeed);
            // }
            
            // Smoothing//
            if (rotations.Count >= smoothingFrames) {
                rotations.Dequeue();
                positions.Dequeue();
            }
        
            rotations.Enqueue(base_link.transform.rotation);
            positions.Enqueue(base_link.transform.position);

            avgr = Vector4.zero;
            foreach (Quaternion singleRotation in rotations) {
                Math3d.AverageQuaternion(ref avgr, singleRotation, rotations.Peek(), rotations.Count);
            }

            avgp = Vector3.zero;
            foreach (Vector3 singlePosition in positions) {
                avgp += singlePosition;
            }
            avgp /= positions.Count;

            smoothedRotation = new Quaternion(avgr.x, avgr.y, avgr.z, avgr.w);
            smoothedPosition = avgp;
            //**//

            // hardcoded offset to align ground in XR with real world ground //
            // if (aligned_base_link.position.x <= 80) { LiDAR_Roatation_Offset = Quaternion.Euler(8, 0, 0); }
            // else { LiDAR_Roatation_Offset = Quaternion.Euler(-8, 0, 0); }
            // transform.localPosition = new Vector3(LiDARToCarX, LiDARToCarY, LiDARToCarZ);
            // transform.localRotation = global_Roatation_Offset * LiDAR_Roatation_Offset;
            //**//
        }
    }

    private void LateUpdate() {
        aligned_base_link.position = smoothedPosition;
        aligned_base_link.rotation = LiDAR_Roatation_Offset * smoothedRotation;
        // car.position = aligned_base_link.position + new Vector3(LiDARToCarX, LiDARToCarY, LiDARToCarZ);;
        // car.rotation = aligned_base_link.rotation * global_Roatation_Offset * LiDAR_Roatation_Offset;
        car.localPosition = new Vector3(LiDARToCarX, LiDARToCarY, LiDARToCarZ);
        car.localRotation = global_Roatation_Offset * LiDAR_Roatation_Offset;
    }

    // void PointCallback(PointMsg msg) {
    //     LiDAR_Translation_Offset = msg.From<FLU>();
    //     Debug.Log(LiDAR_Translation_Offset);
    // }
    //
    // void QuaternionCallback(QuaternionMsg msg) {
    //     LiDAR_Roatation_Offset = msg.From<FLU>();
    //     Debug.Log(LiDAR_Roatation_Offset);
    // }
}


