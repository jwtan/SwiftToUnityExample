#import <UnityFramework/UnityFramework-Swift.h>

extern "C"
{
    void UnityOnStart()
    {
        [[SwiftToUnity shared]   UnityOnStart];
    }
}
