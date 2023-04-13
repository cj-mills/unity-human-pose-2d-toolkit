using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJM.HumanPose2DToolkit
{
    /// <summary>
    /// Represents a single body part in 2D space with its index, coordinates, and probability.
    /// </summary>
    public struct BodyPart2D
    {
        public int index; // The index of the body part
        public Vector2 coordinates; // The 2D coordinates of the body part
        public float prob; // The probability of the detected body part

        /// <summary>
        /// Initializes a new instance of the BodyPart2D struct.
        /// </summary>
        /// <param name="index">The index of the body part.</param>
        /// <param name="coordinates">The 2D coordinates of the body part.</param>
        /// <param name="prob">The probability of the detected body part.</param>
        public BodyPart2D(int index, Vector2 coordinates, float prob)
        {
            this.index = index;
            this.coordinates = coordinates;
            this.prob = prob;
        }
    }

    /// <summary>
    /// Represents a detected human pose in 2D space with its index and an array of body parts.
    /// </summary>
    public struct HumanPose2D
    {
        public int index; // The index of the detected human pose
        public BodyPart2D[] bodyParts; // An array of the body parts that make up the human pose

        /// <summary>
        /// Initializes a new instance of the HumanPose2D struct.
        /// </summary>
        /// <param name="index">The index of the detected human pose.</param>
        /// <param name="bodyParts">An array of body parts that make up the human pose.</param>
        public HumanPose2D(int index, BodyPart2D[] bodyParts)
        {
            this.index = index;
            this.bodyParts = bodyParts;
        }
    }

    public static class HumanPose2DUtility
    {

        /// <summary>
        /// Scales and optionally mirrors the coordinates of a body part in a pose skeleton to match the in-game screen and display resolutions.
        /// </summary>
        /// <param name="coordinates">The (x,y) coordinates for a BodyPart object.</param>
        /// <param name="inputDims">The dimensions of the input image used for pose estimation.</param>
        /// <param name="screenDims">The dimensions of the in-game screen where the body part will be displayed.</param>
        /// <param name="offset">An offset to apply to the body part coordinates when scaling.</param>
        /// <param name="mirrorScreen">A boolean flag to indicate if the body part coordinates should be mirrored horizontally (default is false).</param>
        public static Vector2 ScaleBodyPartCoords(Vector2 coordinates, Vector2Int inputDims, Vector2 screenDims, Vector2Int offset, bool mirrorScreen)
        {
            // The smallest dimension of the screen
            float minScreenDim = Mathf.Min(screenDims.x, screenDims.y);
            // The smallest input dimension
            int minInputDim = Mathf.Min(inputDims.x, inputDims.y);
            // Calculate the scale value between the in-game screen and input dimensions
            float minImgScale = minScreenDim / minInputDim;
            // Calculate the scale value between the in-game screen and display
            float displayScaleX = Screen.width / screenDims.x;
            float displayScaleY = Screen.height / screenDims.y;
            float displayScale = Mathf.Min(displayScaleX, displayScaleY);


            // Scale body part coordinates to in-game screen resolution and flip the coordinates vertically
            float x = (coordinates.x + offset.x) * minImgScale;
            float y = (inputDims.y - (coordinates.y - offset.y)) * minImgScale;

            // Mirror bounding box across screen
            if (mirrorScreen)
            {
                x = screenDims.x - x;
            }

            // Scale coordinates to display resolution
            coordinates.x = x * displayScale;
            coordinates.y = y * displayScale;

            // Offset the coordinates coordinates based on the difference between the in-game screen and display
            coordinates.x += (Screen.width - screenDims.x * displayScale) / 2;
            coordinates.y += (Screen.height - screenDims.y * displayScale) / 2;

            return coordinates;
        }
    }
}
