using System.Windows.Forms;

namespace Crom.Controls
{
   /// <summary>
   /// Differences between Windows Forms implementations that the controls need to work around.
   /// Mono's own implementation (used on Linux and macOS) differs from the .NET Framework in these
   /// places. Wine Mono uses the .NET implementation, so behaves like the .NET Framework.
   /// </summary>
   internal static class WinFormsBehaviour
   {
      private static readonly bool IsMonoImplementation =
         typeof(Control).Assembly.GetType("System.Windows.Forms.XplatUI") != null;

      /// <summary>
      /// Gets whether <see cref="Control.CreateControlsInstance"/> is called from within the base
      /// <see cref="Control"/> constructor (before the owner's constructor runs), where child controls
      /// can not yet be added to the collection. The .NET implementation creates the collection later,
      /// and the docking controls depend on their children being added at that point.
      /// </summary>
      public static bool CreatesControlCollectionInBaseConstructor => IsMonoImplementation;

      /// <summary>
      /// Gets whether <see cref="Control.VisibleChanged"/> is reliably raised when a control becomes
      /// visible because of a change to its parent.
      /// </summary>
      public static bool RaisesVisibleChangedForParentChanges => !IsMonoImplementation;

      /// <summary>
      /// Gets whether controls that are created, docked and resized while their container's layout is
      /// suspended are drawn once layout resumes. On Mono they are laid out and receive paint events,
      /// but nothing they draw appears on screen.
      /// </summary>
      public static bool DrawsControlsAddedWhileLayoutSuspended => !IsMonoImplementation;
   }
}
