### Portobello

Portobello is an infrastructure system that enables study portability in driving simulation studies. It consisted of several key components that rely on open-sourced hardware.

<img src="images/xr-oom-hardware.png"/>

---

#### Hardware Setup
Only core equipment is listed.
- LiDAR (Ouster-64)
- IMU (if not build in the LiDAR, an external IMU is required, we had Xsens MTi 300)
- Laptop (Ubuntu 20.04, running ROS)
- Desktop (Windows, running Unity)
- Ethernet Switch 
- Mobile Router

Mobile Router is required to assign IP address for devices, including LiDAR, desktop running Unity, and the laptop running ROS. 
 
---

#### Software Setup
- Robot Operating System (ROS 1 Noetic)
- Unity


#### Mapping
To generate a map of the study area, we used SLAM algorithms to generate a digital twin pointcloud. The algorithm we used was [LIO-SAM](https://github.com/TixiaoShan/LIO-SAM) by [Tianxiao Shan](https://github.com/TixiaoShan), but there are many alternatives online. Users should choose based on their system and available sensors. 

To visualize point cloud in Unity for event staging, we recommend the [pcx](https://github.com/keijiro/Pcx) shader made by [Keijiro Takahashi](https://github.com/keijiro).

#### Localization
At runtime, to localize the vehicle's position in the pointcloud, we used [hdl_localization](https://github.com/koide3/hdl_localization). The key is to have a tf transform from the `/map` frame to `/base_link` frame or `/lidar_link` frame.

#### Connecting with Unity
To connect ROS tf tree with Unity, [Unity-Robotics-Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) provides a bridge between the two worlds. Specifically, we used the [ROS-TCP-IP](https://github.com/Unity-Technologies/ROS-TCP-Connector) package to broadcast tf tree to Unity. Frames in tf trees will show up as transforms in Unity object hierarchy, 

#### Attaching Gameobjects to Real-World Object
Since the LiDAR's real world position is available in Unity as transforms, users can attach objects to these transforms as children, or keep a history of the past coordinates for smoothing.
