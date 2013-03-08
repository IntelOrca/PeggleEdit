using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IntelOrca.PeggleEdit.Designer
{
	class EditorContext
	{
		private PegglePakCollection mPakCollection;
		private Level mLevel;
		private PakRecord mLevelPakRecord;

		private readonly DisplayOptions mDisplayOptions = new DisplayOptions();

		public event EventHandler OnPakCollectionChanged;
		public event EventHandler OnLevelChanged;

		public EditorContext()
		{
		}

		public BitmapSource GetBitmapImage(string path)
		{
			return GetBitmapImage(path, false);
		}

		private BitmapSource GetBitmapImage(string path, bool alpha)
		{
			PakRecord record = mPakCollection.GetImageRecord(path);
			if (record == null)
				return null;
			path = record.FileName;

			byte[] data;
			if (Path.GetExtension(path).Equals(".j2k", StringComparison.OrdinalIgnoreCase) ||
					Path.GetExtension(path).Equals(".jp2", StringComparison.OrdinalIgnoreCase)) {
				data = GetBitmapImageFromJ2K(record.Buffer, Path.GetExtension(path));
			} else {
				data = new byte[record.Buffer.Length];
				Array.Copy(record.Buffer, data, data.Length);
			}

			BitmapSource result;
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.StreamSource = new MemoryStream(data);
			bitmap.EndInit();
			result = bitmap;

			// Check if there is an alpha image
			if (!alpha) {
				BitmapSource alphaBitmap = GetBitmapImage(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_"), true);
				if (alphaBitmap != null)
					result = CombineColourAndAlpha(bitmap, alphaBitmap);
			}

			return result;
		}

		private byte[] GetBitmapImageFromJ2K(byte[] data, string extension)
		{
			string tempPath = Path.ChangeExtension(Path.GetTempPath() + Path.GetRandomFileName(), extension);
			File.WriteAllBytes(tempPath, data);
			OpenJPEG.CallJ2K(tempPath, tempPath + ".bmp");
			File.Delete(tempPath);
			data = File.ReadAllBytes(tempPath + ".bmp");
			File.Delete(tempPath + ".bmp");
			return data;
		}

		private BitmapSource CombineColourAndAlpha(BitmapSource colour, BitmapSource alpha)
		{
			int width = colour.PixelWidth;
			int height = colour.PixelHeight;

			// Create opacity mask
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen())
				drawingContext.DrawImage(alpha, new Rect(0, 0, width, height));
			RenderTargetBitmap alphaRTB = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			alphaRTB.Render(drawingVisual);
			WriteableBitmap alphaWB = new WriteableBitmap(alphaRTB);
			alphaWB.Lock();
			for (int y = 0; y < alphaWB.PixelHeight; y++) {
				for (int x = 0; x < alphaWB.PixelWidth; x++) {
					IntPtr dst = alphaWB.BackBuffer + (y * alphaWB.BackBufferStride) + x * 4;
					Marshal.WriteByte(dst + 3, Marshal.ReadByte(dst));
				}
			}
			alphaWB.AddDirtyRect(new Int32Rect(0, 0, width, height));
			alphaWB.Unlock();
			ImageBrush opacityMask = new ImageBrush(alphaWB);

			// Draw final image
			drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				drawingContext.PushOpacityMask(opacityMask);
				drawingContext.DrawImage(colour, new Rect(0, 0, width, height));
				drawingContext.Pop();
			}
			RenderTargetBitmap finalRTB = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
			finalRTB.Render(drawingVisual);

			return finalRTB;
		}

		public void LoadLevel(PakRecord record)
		{
			LevelReader reader = new LevelReader(record.Buffer);
			Level level = reader.Read();
			if (level == null) {
				// throw new Exception();
				return;
			}

			mLevelPakRecord = record;
			LoadLevel(level);
		}

		public void LoadLevel(Level level)
		{
			mLevel = level;
			if (OnLevelChanged != null)
				OnLevelChanged.Invoke(this, EventArgs.Empty);
		}

		public PegglePakCollection PakCollection
		{
			get { return mPakCollection; }
			set {
				mPakCollection = value;
				if (OnPakCollectionChanged != null)
					OnPakCollectionChanged.Invoke(this, EventArgs.Empty);
			}
		}

		public Level Level
		{
			get { return mLevel; }
		}

		public PakRecord LevelPakRecord
		{
			get { return mLevelPakRecord; }
			set { mLevelPakRecord = value; }
		}

		public DisplayOptions DisplayOptions
		{
			get { return mDisplayOptions; }
		}
	}
}
