# Swift To Unity iOS Example
This example project demonstrates how to setup your Xcode project so that your Swift code is able to call Unity's Objective-C methods (e.g. `UnitySendMessage`), particularly in Unity 2019.3 and later.

## Unity 2019.2 and earlier
If you're running Unity 2019.2 and earlier, the solution is simple. You just need a [Bridging Header](https://developer.apple.com/documentation/swift/imported_c_and_objective-c_apis/importing_objective-c_into_swift) that imports `UnityInterface.h`. This will allow your Swift code to call `UnitySendMessage` without any further modifications.

## Unity 2019.3 and later
If you're running Unity 2019.3 and later, things get more complicated. The structure of the generated [Xcode project](https://docs.unity3d.com/Manual/StructureOfXcodeProject.html) has been changed so as to accommodate the Unity as a Library feature. The most important change is that all Unity functionality has been moved into a framework target called UnityFramework.

### Bridging Header
Let's try our original approach -- the Bridging Header. Since all Unity functionality is contained in the UnityFramework target, we'll reference the Bridging Header in the Build Settings for this target.

Running a build at this point reveals a problem:
```
<unknown>:0: error: using bridging headers with framework targets is unsupported
```
It looks like we'll have to try something else.

### Umbrella Header
Fortunately, the [Bridging Header](https://developer.apple.com/documentation/swift/imported_c_and_objective-c_apis/importing_objective-c_into_swift) document also includes a section about importing code within a framework target.

1. Set the `DEFINES_MODULE` build setting for UnityFramework to `Yes`.
2. In the Umbrella Header of UnityFramework (i.e. `UnityFramework/UnityFramework.h`), import the Objective-C header that we want to expose to Swift (i.e. `#import "UnityInterface.h"`).

Running a build at this point reveals a new problem:
```
Include of non-modular header inside framework module 'UnityFramework':'.../UnityInterface.h'
```

Researching this error message, it appears that `UnityInterface.h` needs to be made publicly available as part of the framework's public headers. These are the steps:

1. In the Project Navigator view, select `Classes/Unity/UnityInterface.h`
2. Go to the Inspectors panel (right side of the screen) and select the File Inspector tab
3. In the Target Membership section, enable the checkbox for UnityFramework and set its scope to `Public`.

We'll also need to set these settings recursively for the header files that `UnityInterface.h` itself references. Here is the full list of header files that we need to modify:
* `Classes/Unity/UnityInterface.h`
* `Classes/Unity/UnityForwardDecls.h`
* `Classes/Unity/UnityRendering.h`
* `Classes/Unity/UnitySharedDecls.h`

Running another build and... it builds successfully! At this point, we already have a working integration. However, there is another possible solution...

### Module Map
While researching this problem, I came across some suggestions about using a Module Map.

Swift [documentation](https://swift.org/swift-compiler/#compiler-architecture) states that the Swift compiler has the ability to import [Clang Modules](http://clang.llvm.org/docs/Modules.html) and map the Objective-C APIs they export into their corresponding Swift APIs. Fortunately, Xcode already treats framework targets as Modules so we just need to define a [Module Map](https://clang.llvm.org/docs/Modules.html#module-maps) for UnityFramework that defines a [Submodule](https://clang.llvm.org/docs/Modules.html#submodule-declaration) for `UnityInterface.h`.

The default Module Map for UnityFramework looks like this:
```
framework module UnityFramework {
  umbrella header "UnityFramework.h"

  export *
  module * { export * }
}
```
We'll then define a Submodule in it like this :
```
framework module UnityFramework {
  umbrella header "UnityFramework.h"

  export *
  module * { export * }

  module UnityInterface {
      header "UnityInterface.h"

      export *
  }
}
```

Proceed to complete the setup by doing the following:
- Save the modified Module Map in a file with the extension `.modulemap`.
- Add this file to your Xcode project.
- Reference this file in the `MODULEMAP_FILE` build setting for UnityFramework.
- Set Target Membership settings recursively for `UnityInterface.h` and the header files it references.

Overall, this method seems to be a better approach as it's cleaner and you avoid having to do something intrusive like manually editing the Umbrella Header (i.e. `UnityFramework.h`).

## Key Files

### UnityFramework.modulemap
Module Map file that defines a Submodule for `UnityInterface.h`.

### SwiftToUnityPostProcess.cs
Build post process script that automatically implements the integration steps:

- Set the `DEFINES_MODULE` build setting for UnityFrameworks to `Yes`.
- Copy and add the `UnityFramework.modulemap` file to the Xcode project.
- Reference `UnityFramework.modulemap` in the `MODULEMAP_FILE` build setting for UnityFrameworks.
- Add the following headers to the UnityFramework target and set their scope to `Public`:
-- `Classes/Unity/UnityInterface.h`
-- `Classes/Unity/UnityForwardDecls.h`
-- `Classes/Unity/UnityRendering.h`
-- `Classes/Unity/UnitySharedDecls.h`

## Example Files

### CubeScript.cs
Example monobehaviour script that calls `SwiftToUnityBridge.mm` during `Start()` and receives messages sent from `SwiftToUnity.swift`.

### SwiftToUnityBridge.mm
Example Objective-C script that is called by `CubeScript.cs` during `Start()` which triggers `SwiftToUnity.swift` to call `UnitySendMessage`.

### SwiftToUnity.swift
Example Swift script that calls `UnitySendMessage`.
