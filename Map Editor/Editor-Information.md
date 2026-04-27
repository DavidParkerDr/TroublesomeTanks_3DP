# Troublesome Tanks Map Editor

For the game a map editor was created. This is a separate application which will allow you to create maps to be used within the game.

## How to setup
Add the folder to a common folder which also has a folder which contains the tankontroller which holds the game. Load the editor executable. When Cloning the repository, this layout will already be in place, so the map loading will work.

## The controls

### Keyboard

These are the controls for the map editor when using a keyboard, which is currently the only control option.

![alt text](MapEditor_Controls.png)

- Uses drag and drop system in editing scene.
- Objects selected by clicking and holding on the object with the mouse.
- When an object is selected can interact with them using the keyboard.
- Can navigate the menu using arrow keys.
- Left/Right move in that direction in the menu if supported.
- Up/Down move in that direction in the menu if supported.
- Pressing enter will select the menu option.

### Solution Diagrams

### Scene Management - Class Diagram

``` mermaid

classDiagram

direction LR

class IScene {
    <<abstract>>
    +Draw(float pSeconds)
    +Update(float pSeconds)
    +Escape()
}

class MainMenuScene
class MapSelectionScene
class MapEditingScene
class TransitionScene

IScene <|-- MainMenuScene
IScene <|-- MapSelectionScene
IScene <|-- MapEditingScene
IScene <|-- TransitionScene


class SceneManager {
    +Instance SceneManager
    +Top IScene
    +Previous IScene
    +Push(IScene pScene)
    +Transition(IScene pNextScene, bool pReplaceCurrent)
    +Pop()
    +Update(float pSeconds)
    +Draw(float pSeconds)
}

SceneManager o-- IScene

SceneManager ..> TransitionScene

MainMenuScene ..> MapSelectionScene
MainMenuScene ..> MapEditingScene
MapSelectionScene --> IScene
MapEditingScene --> IScene

```


### SceneObject Interaction - Class Diagram

```mermaid
classDiagram

direction LR

class SelectionManager {
    +HandleInteraction(Vector2 pMousePosition)
    +GetSelectedObject() SceneObject
    +SetSelectedObject(SceneObject pSceneObject)
}

class SceneObject {
    <<abstract>>
    +mRectangle Rectangle
    +Draw(SpriteBatch)
    +DrawOutline(SpriteBatch)
    +IsPointWithin(Vector2 point) bool
    +UpdatePosition(int x, int y)
    +SetRectangle(Rectangle rectangle)
    +SetSelected(bool selected)
    +GetIsSelected() bool
}

class RectWall {
    +mRotation float
    +Rotate(float pDelta)
    +ScaleWidth(float pScale)
    +ScaleHeight(float pScale)
    +SwitchRotationScaling()
}

class Tank {
    +mRotation float
    +Rotate(float delta)
}

class Pickup {
    +TogglePickupType(PickupType type)
    +SetActivatedPickups(Dictionary~PickupType,bool~)
    +GetActivatedPickups() Dictionary~PickupType,bool~
}

class PickupType {
    <<enumeration>>
}

class TemplatePalette {
    +IsDraggingAny bool
    +Update(Vector2 pMousePosition)
    +Draw(SpriteBatch pSpriteBatch)
}

class DraggableTemplate {
    +mTemplate T
    +mIsDragging bool
    +BeginDrag(Vector2 pMousePosition)
    +Update(Vector2 pMousePosition)
    +EndDrag(bool pResetToOriginal) Rectangle
    +Reset()
}

class MapBoundaryValidator {
    +IsRectWithinPlayArea(Rectangle pRect) bool
    +IsWallWithinPlayArea(RectWall pWall) bool
}

class MapPreview {
    +GetPlayArea() Rectangle
    +GetWalls() List~RectWall~
    +GetTanks() List~Tank~
    +GetPickups() List~Pickup~
    +AddObject(SceneObject pObject)
    +RemoveObject(SceneObject pObject)
    +SaveMap(string pMapName)
    +MapDataFromPreview()
}


SelectionManager --> MapBoundaryValidator
SelectionManager --> TemplatePalette

TemplatePalette o-- DraggableTemplate
TemplatePalette --> MapPreview
TemplatePalette --> MapBoundaryValidator



SceneObject <|-- RectWall
SceneObject <|-- Tank
SceneObject <|-- Pickup


Pickup --> PickupType
```

### Map Editing Service - Class Diagram

``` mermaid

classDiagram

direction LR

class FileNamer {
    +Update(float pDeltaTime)
    +Draw(SpriteBatch pSpriteBatch)
    +ReturnName() string
    +StartTyping()
    +IsActive() bool
}


class MapEditingMapService {
    +CreatePreviewForExistingMap(string pMapFile) MapPreview
    +CreatePreviewForNewMap(string pMapFile) MapPreview
    +SaveMap(MapPreview pPreview, string pName)
}

class MapPreview


class MapData {
    +Walls List~WallData~
    +Tanks List~TankData~
    +Pickups List~PickupData~
}

class WallData {
    +Texture string
    +Position string[]
    +Size string[]
    +Rotation string
}

class TankData {
    +Position string[]
    +Rotation string
}

class PickupData {
    +Position string[]
    +ActivatedPickups Dictionary~PickupType,bool~
}

MapEditingMapService --> MapPreview
MapPreview --> MapData

MapData o-- WallData
MapData o-- TankData
MapData o-- PickupData

```
