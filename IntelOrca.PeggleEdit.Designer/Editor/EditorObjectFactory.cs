using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer.Editor
{
	static class EditorObjectFactory
	{
		public static EditorObject FromLevelEntry(EditorContext editor, LevelEntry le)
		{
			switch (le.Type) {
			case Circle.ClassType: return new CircleEditorObject(editor, le);
			case Brick.ClassType: return new BrickEditorObject(editor, le);
			case Polygon.ClassType: return new PolygonEditorObject(editor, le);
			default: return new EditorObject(editor, le);
			}
		}
	}
}
