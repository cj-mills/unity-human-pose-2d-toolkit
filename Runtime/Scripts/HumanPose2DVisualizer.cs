using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CJM.HumanPose2DToolkit
{
    public class HumanPose2DVisualizer : MonoBehaviour
    {
        // Main canvas to display poses
        [Header("UI Components")]
        [Tooltip("The main canvas to display poses")]
        [SerializeField] private Canvas canvas;

        // Prefabs for pose containers, joints, and bones
        [Tooltip("The prefab for the pose container, which holds the joints and bones")]
        [SerializeField] private RectTransform poseContainerPrefab;
        [Tooltip("The prefab for the joint image")]
        [SerializeField] private Image jointPrefab;
        [Tooltip("The prefab for the bone RectTransform")]
        [SerializeField] private RectTransform bonePrefab;

        // Configuration and styling
        [Header("Configuration")]
        [Tooltip("The JSON file containing body part connection information")]
        [SerializeField] private TextAsset bodyPartConnectionsFile;
        [Tooltip("The color of the bones")]
        [SerializeField] private Color boneColor = Color.green;
        [Tooltip("The color of the joints")]
        [SerializeField] private Color jointColor = Color.green;

        // Serializable classes to store body part connection information from JSON
        [System.Serializable]
        class BodyPartConnection
        {
            public int from; // Index of the starting body part
            public int to;   // Index of the ending body part
        }

        [System.Serializable]
        class BodyPartConnectionList
        {
            public List<BodyPartConnection> bodyPartConnections; // List of body part connections
        }

        // Variables to store runtime instances and data
        private List<BodyPartConnection> bodyPartConnections; // List of body part connections
        private List<RectTransform> poseContainers = new List<RectTransform>(); // List of instantiated pose containers
        private List<List<Image>> joints = new List<List<Image>>(); // Nested list of instantiated joint images
        private List<List<RectTransform>> bones = new List<List<RectTransform>>(); // Nested list of instantiated bone RectTransforms
        private float confidenceThreshold; // Confidence threshold for displaying poses

        // GUIDs of the default assets
        private const string PoseContainerPrefabGUID = "12c840be0a8d4adc879fc14fb79a316d";
        private const string JointPrefabGUID = "d90f7f2e5b8f4daa885f9441f0f33427";
        private const string BonePrefabGUID = "ed947d23b5354617b130aa8ee0cc610b";
        private const string BodyPartConnectionsFileGUID = "0fc008c60a8e44589674b0f455384a5b";


        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's context menu
        /// or when adding the component the first time. This function is only called in editor mode.
        /// </summary>
        private void Reset()
        {
            // Load default assets only in the Unity Editor, not in a build
#if UNITY_EDITOR
            poseContainerPrefab = LoadDefaultAsset<RectTransform>(PoseContainerPrefabGUID);
            jointPrefab = LoadDefaultAsset<Image>(JointPrefabGUID);
            bonePrefab = LoadDefaultAsset<RectTransform>(BonePrefabGUID);
            bodyPartConnectionsFile = LoadDefaultAsset<TextAsset>(BodyPartConnectionsFileGUID);
#endif
        }


        /// <summary>
        /// Loads the default asset for the specified type using its GUID.
        /// </summary>
        /// <typeparam name="T">The type of asset to be loaded.</typeparam>
        /// <param name="guid">The GUID of the default asset.</param>
        /// <returns>The loaded asset of the specified type.</returns>
        /// <remarks>
        /// This method is only executed in the Unity Editor, not in builds.
        /// </remarks>
        private T LoadDefaultAsset<T>(string guid) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            // Load the asset from the AssetDatabase using its GUID
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
            return null;
#endif
        }


        private void Start()
        {
            LoadBodyPartConnectionList();
        }

        /// <summary>
        /// Load the JSON file
        /// <summary>
        private void LoadBodyPartConnectionList()
        {
            if (IsJsonNullOrEmpty())
            {
                Debug.LogError("JSON file is null or empty.");
                return;
            }

            bodyPartConnections = DeserializeBodyPartConnectionsList(bodyPartConnectionsFile.text).bodyPartConnections;
        }

        /// <summary>
        /// Check if JSON file is null or empty
        /// <summary>
        private bool IsJsonNullOrEmpty()
        {
            return bodyPartConnectionsFile == null || string.IsNullOrWhiteSpace(bodyPartConnectionsFile.text);
        }

        /// <summary>
        /// Deserialize the JSON string
        /// <summary>
        private BodyPartConnectionList DeserializeBodyPartConnectionsList(string json)
        {
            try
            {
                return JsonUtility.FromJson<BodyPartConnectionList>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize class labels JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates the pose visualizations based on the provided human poses and a confidence threshold.
        /// </summary>
        /// <param name="humanPoses">An array of human poses to visualize</param>
        /// <param name="confidenceThreshold">The minimum confidence required to display a pose (default is 0.5f)</param>
        public void UpdatePoseVisualizations(HumanPose2D[] humanPoses, float confidenceThreshold = 0.5f)
        {
            this.confidenceThreshold = confidenceThreshold;

            // Instantiate pose containers, joint images, and bone RectTransforms as needed to match the number of humanPoses
            while (poseContainers.Count < humanPoses.Length)
            {
                RectTransform newPoseContainer = Instantiate(poseContainerPrefab, canvas.transform);
                poseContainers.Add(newPoseContainer);
                joints.Add(new List<Image>());
                bones.Add(new List<RectTransform>());
            }

            for (int i = 0; i < poseContainers.Count; i++)
            {
                if (i < humanPoses.Length)
                {
                    // Get references to joint and bone containers for the current pose
                    RectTransform jointContainer = poseContainers[i].Find("JointContainer").GetComponent<RectTransform>();
                    RectTransform boneContainer = poseContainers[i].Find("BoneContainer").GetComponent<RectTransform>();

                    // Update the joint positions and visibility
                    UpdateJoints(humanPoses[i].bodyParts, jointContainer, joints[i]);
                    // Update the bone positions, rotations, and visibility
                    UpdateBones(humanPoses[i].bodyParts, boneContainer, joints[i], bones[i]);

                    // Set the pose container active
                    poseContainers[i].gameObject.SetActive(true);
                }
                else
                {
                    // Set the pose container inactive for unused containers
                    poseContainers[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Converts a screen point to a local point within the given canvas RectTransform.
        /// </summary>
        /// <param name="canvas">The canvas RectTransform to convert the point to</param>
        /// <param name="screenPoint">The screen point to convert</param>
        /// <returns>A Vector2 representing the local point within the canvas RectTransform</returns>
        private Vector2 ScreenToCanvasPoint(RectTransform canvas, Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }


        /// <summary>
        /// Updates the joint visualizations based on the provided body parts, adjusting their positions and visibility.
        /// </summary>
        /// <param name="bodyParts">An array of body parts containing position and probability data</param>
        /// <param name="jointContainer">The RectTransform containing joint images</param>
        /// <param name="jointsList">A list of instantiated joint images</param>
        private void UpdateJoints(BodyPart2D[] bodyParts, RectTransform jointContainer, List<Image> jointsList)
        {
            // Instantiate joint images as needed to match the number of bodyParts
            while (jointsList.Count < bodyParts.Length)
            {
                Image newJoint = Instantiate(jointPrefab, jointContainer);
                jointsList.Add(newJoint);
            }

            for (int i = 0; i < jointsList.Count; i++)
            {
                if (bodyParts[i].prob >= confidenceThreshold)
                {
                    Image joint = jointsList[i];
                    RectTransform jointRect = joint.rectTransform;
                    // Update joint position
                    jointRect.anchoredPosition = ScreenToCanvasPoint(jointContainer, bodyParts[i].coordinates);
                    // Update joint color
                    joint.color = jointColor;
                    // Set the joint game object active
                    joint.gameObject.SetActive(true);
                }
                else
                {
                    // Set the joint game object inactive if below the confidence threshold
                    jointsList[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates the bone visualizations based on the provided body parts and joint positions, adjusting their positions, rotations, and visibility.
        /// </summary>
        /// <param name="bodyParts">An array of body parts containing position and probability data</param>
        /// <param name="boneContainer">The RectTransform containing bone RectTransforms</param>
        /// <param name="jointsList">A list of instantiated joint images</param>
        /// <param name="bonesList">A list of instantiated bone RectTransforms</param>
        private void UpdateBones(BodyPart2D[] bodyParts, RectTransform boneContainer, List<Image> jointsList, List<RectTransform> bonesList)
        {
            // Instantiate bone RectTransforms as needed to match the number of bodyPartConnections
            while (bonesList.Count < bodyPartConnections.Count)
            {
                RectTransform newBone = Instantiate(bonePrefab, boneContainer);
                bonesList.Add(newBone);
            }

            for (int i = 0; i < bonesList.Count; i++)
            {
                Image fromJoint = jointsList[bodyPartConnections[i].from];
                Image toJoint = jointsList[bodyPartConnections[i].to];

                // If both connected joints are active, display the bone
                if (fromJoint.IsActive() && toJoint.IsActive())
                {
                    RectTransform bone = bonesList[i];
                    Vector2 fromJointPos = bodyParts[bodyPartConnections[i].from].coordinates;
                    Vector2 toJointPos = bodyParts[bodyPartConnections[i].to].coordinates;
                    Vector2 direction = toJointPos - fromJointPos;
                    float distance = direction.magnitude;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // Update bone size based on the distance between joints
                    bone.sizeDelta = new Vector2(distance, bone.sizeDelta.y);

                    // Calculate the bone position and update it
                    Vector2 bonePos = new Vector2((fromJointPos.x + toJointPos.x) / 2, (fromJointPos.y + toJointPos.y) / 2);
                    bone.anchoredPosition = ScreenToCanvasPoint(boneContainer, bonePos);

                    // Update bone rotation based on the angle between joints
                    bone.localEulerAngles = new Vector3(0, 0, angle);
                    bone.GetComponent<Image>().color = boneColor;
                    // Set the bone game object active
                    bone.gameObject.SetActive(true);
                }
                else
                {
                    // Set the bone game object inactive if below the confidence threshold
                    bonesList[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
