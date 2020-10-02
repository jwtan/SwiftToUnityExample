import Foundation

@objc public class SwiftToUnity: NSObject
{
    @objc public static let shared = SwiftToUnity()

    @objc public func UnityOnStart()
    {
        UnitySendMessage("Cube", "OnMessageReceived", "Hello World!");
    }
}
