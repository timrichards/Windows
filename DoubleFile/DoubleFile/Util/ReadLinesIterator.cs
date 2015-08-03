using System.IO;
using System.Text;

namespace DoubleFile
{
	internal class ReadLinesIterator : Iterator<string>
	{
		private StreamReader _reader;

		private ReadLinesIterator(StreamReader reader)
		{
			_reader = reader;
		}

		public override bool MoveNext()
		{
			if (_reader != null)
			{
				current = _reader.ReadLine();
				if (current != null)
				{
					return true;
				}
				base.Dispose();
			}
			return false;
		}

		protected override Iterator<string> Clone()
		{
			return CreateIterator(_reader);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _reader != null)
				{
					_reader.Dispose();
				}
			}
			finally
			{
				_reader = null;
				base.Dispose(disposing);
			}
		}

		internal static ReadLinesIterator CreateIterator(StreamReader reader)
		{
			return new ReadLinesIterator(reader);
		}
	}
}
