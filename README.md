# Unity Human Pose 2D Toolkit
This Unity package provides an easy-to-use and customizable solution to work with and visualize 2D human poses on a Unity canvas.

## Demo Video
https://user-images.githubusercontent.com/9126128/231926454-ae40f842-7b90-4fa9-97f3-245d9d788ee5.mp4



## Demo Projects

| GitHub Repository                                            | Description                                                |
| ------------------------------------------------------------ | ---------------------------------------------------------- |
| [barracuda-inference-posenet-demo](https://github.com/cj-mills/barracuda-inference-posenet-demo) | Perform 2D human pose estimation using PoseNet models.     |



## Features

- Display 2D pose skeletons with customizable transparency and colors
- Automatically manage and update UI elements based on provided pose data
- Compatible with Unity UI

## Getting Started

### Prerequisites

- Unity game engine

### Installation

You can install the Human Pose 2D Toolkit package using the Unity Package Manager:

1. Open your Unity project.
2. Go to Window > Package Manager.
3. Click the "+" button in the top left corner, and choose "Add package from git URL..."
4. Enter the GitHub repository URL: `https://github.com/cj-mills/unity-human-pose-2d-toolkit.git`
5. Click "Add". The package will be added to your project.

For Unity versions older than 2021.1, add the Git URL to the `manifest.json` file in your project's `Packages` folder as a dependency:

```json
{
  "dependencies": {
    "com.cj-mills.human-pose-2d-toolkit": "https://github.com/cj-mills/unity-human-pose-2d-toolkit.git",
    // other dependencies...
  }
}

```
