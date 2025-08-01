# CAOP

## Installation

1. Install Unity 6000.0.37f1 from the Unity Hub  
2. Clone repository  
3. In the Unity Hub, click "Add project from disk" and select the "CAOP" folder from the repository, then open the project with 6000.0.37f1. If it gives a warning or asks to open in safe mode, ignore.  
4. No packages needed right now

### For Leap Motion:
1. Download and install Ultraleap Hyperion (https://leap2.ultraleap.com/downloads/leap-motion-controller-2/)  
2. In Unity, select Edit -> Project Settings -> Package Manager, add a new scoped registry with the following details:  
    Name: Ultraleap  
    URL: https://package.openupm.com  
    Scope(s): com.ultraleap
3. In the package manager, navigate to “My Registries” in the dropdown at the top left of the window. Add the “Ultraleap Tracking” package by selecting it in the list on the left then clicking install in the bottom right.

## Usage

### Main Scene
* Move Camera: w, a, s, d
* Angle Camera: move mouse
* Exit Movement to click buttons: esc
* Angle the Camera Again: "Resume Movement"
* Click and drag the sphere to set the emphasis point
* Click "Create Chair" and "Create Table" to create those objects
* Click and drag to move them around
* Rotate objects: q, e
* Click "Calculate" to run the MCMC program
* Once it stops, gesture left or right above leap to move the chair

## Known Issues
Sometimes objects get destroyed, I'm still fixing it, just rerun and it should work
