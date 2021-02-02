import AppKit

extension String: Error {}

func getHighestResolution(id: CGDirectDisplayID) -> (width:Int, height:Int) {
    var width: Int = 0;
    var height: Int = 0;

    let modes = CGDisplayCopyAllDisplayModes(id, nil)
    let modesCount = CFArrayGetCount(modes) - 1

    for i in 0...modesCount {
        let mode: CGDisplayMode = unsafeBitCast(CFArrayGetValueAtIndex(modes, i), to: CGDisplayMode.self)

        if (mode.width > width && mode.height > height) {
          width = mode.width;
          height = mode.height;
        }
    }
    return (width: width, height: height);
}

func getScreens(bounds: CGRect) -> [(index:Int, bounds:CGRect, native:(width:Int, height:Int))]? {
    var index = 0
    var out: [(index:Int, bounds:CGRect, native:(width:Int, height:Int))] = [];
    for screen in NSScreen.screens
    {
      let displayID = screen.deviceDescription[NSDeviceDescriptionKey("NSScreenNumber")] as! uint;
      let screenFrame = CGDisplayBounds(displayID);

      if (screenFrame.intersects(bounds))
      {
        let size = getHighestResolution(id: displayID);
        out.append((index: index, bounds: screenFrame, native: size))
      }
      index += 1
    }
    if (out.count == 0) {
      return nil;
    }
    return out;
}

func getPrimaryWindow(pid: pid_t) throws -> (window:[String: Any], bounds:CGRect) {
  let windows = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as! [[String: Any]]

  for window in windows {
    
    let windowOwnerPID = window[kCGWindowOwnerPID as String] as! Int

    if windowOwnerPID != pid {
      continue
    }

    // Skip transparent windows, like with Chrome
    if (window[kCGWindowAlpha as String] as! Double) == 0 {
      continue
    }

    let bounds = CGRect(dictionaryRepresentation: window[kCGWindowBounds as String] as! CFDictionary)!

    // Skip tiny windows, like the Chrome link hover statusbar
    let minWinSize: CGFloat = 50
    if bounds.width < minWinSize || bounds.height < minWinSize {
      continue
    }
    return (window, bounds);
  }
  throw "No matching window found"
}

func getPid() -> pid_t {
	if (CommandLine.arguments.count <= 1 || CommandLine.arguments[1] == "active") {
	  let frontmostApp = NSWorkspace.shared.frontmostApplication!
  	return frontmostApp.processIdentifier;
	} else {
		return Int32(CommandLine.arguments[1])!;
	}
}

func getConfig(pid: pid_t) throws -> String {
  let (window, bounds) = try! getPrimaryWindow(pid: pid);
  let screens = getScreens(bounds: bounds)

  if screens == nil {
    throw "No screens for window"
  }

  let ref = NSRunningApplication(processIdentifier: pid)!;

  let json = """
{
  "title": "\(window[kCGWindowName as String] as? String ?? "")",
  "id": \(window[kCGWindowNumber as String] as! Int),
  "bounds": {
    "x": \(Int(bounds.origin.x)),
    "y": \(Int(bounds.origin.y)),
    "width": \(Int(bounds.width)),
    "height": \(Int(bounds.height))
  },
  "screens": [\(screens!.map {
    """
    {	
      "x": \(Int($0.bounds.minX)),
      "y": \(Int($0.bounds.minY)),
      "width": \(Int($0.bounds.width)),
      "height": \(Int($0.bounds.height)),
      "index": \(Int($0.index)),
      "scale": {
        "x": \(Double($0.native.width)/Double($0.bounds.width)),
        "y": \(Double($0.native.height)/Double($0.bounds.height))
      }
    }
    """
  } .joined(separator: ","))
  ],
  "owner": {
    "name": "\(window[kCGWindowOwnerName as String] as! String)",
    "processId": \(pid),
    "bundleId": "\(ref.bundleIdentifier!)",
    "path": "\(ref.bundleURL!.path)"
  },
  "memoryUsage": \(window[kCGWindowMemoryUsage as String] as! Int)
}
""";

  return json
}

let pid = try! getPid();
let config = try! getConfig(pid: pid)
print(config)
exit(0)
