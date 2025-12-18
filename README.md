# Unity Spatial UI Framework: Dynamic Object Labeling
**Research Project | Augusta University AR/VR Lab** **Developer:** Beyza Koseoglu | **Advisor:** Dr. Jason Orlosky 

## Project Overview
This repository contains a specialized spatial UI system developed for language learning and conceptual acquisition in Virtual Reality (VR).
The core challenge addressed is **view management**: dynamically arranging multiple text labels (e.g., "Chair", "Black", "Furniture") around a 3D object without visual overlap or occlusion.

## Key Features
**Dynamic Non-Overlap Algorithm:** Uses real-time world-space geometry to prevent label intersection.
**Camera-Relative Orientation:** Labels align to the user’s view using camera-relative axes rather than rigid object-space coordinates.
**Scalable Architecture:** Supports arbitrary numbers of labels per object through an iterative layout pipeline.
**High Performance:** Negligible impact on frame rates, optimized for real-time VR hardware.

## Mathematics & Logic
This project leverages a strong mathematical foundation in **3D Geometry** and **Linear Algebra** to handle spatial constraints.

### 1. World-Space Width Estimation
Unlike standard UI which uses pixel-space, this system measures the physical width of **TextMeshPro** strings in the Unity world-space using "marker spheres" placed at the rendered start and end positions of the text.

$$charWidth = \frac{Distance(leftSphere, rightSphere)}{numOfChars}$$

**$charWidth$**: The average world-space width of a single character
**$numOfChars$**: The character count of the baseline text

### 2. Group Centering & Iteration
The system maintains visual symmetry by calculating a total width ($totalWidth$) for $N$ labels, including configurable gaps, and centering the group on the object's world-space center.

$$totalWidth = \sum_{i=1}^{N} width_{i} + gap \times (N - 1)$$

The starting horizontal position ($startX$) is calculated as:

$$startX = centerX - \frac{totalWidth}{2}$$

### 3. Collision Avoidance Logic
Each label is bounded by **"check-spheres"** that represent the adjusted bounding box.
The layout algorithm iterates left-to-right along the camera's right axis to ensure each canvas is perfectly spaced.

## Technical Implementation
**Unity & C# Scripting:** Core logic for geometry calculations and canvas repositioning.
**TextMeshPro (TMP):** Leveraged for high-fidelity text rendering in 3D space.
**Git Workflow:** Managed with clean `.gitignore` protocols to exclude build artifacts (Library, Temp, etc.), focusing on source code integrity.

## Future Work: Crown Labeling
A proposed **"Crown Labeling"** extension will support larger conceptual vocabularies by arranging labels in a multi-row, arc-shaped layout above objects. This utilizes world-space up-axis offsets to maintain high readability for complex data sets.

---
