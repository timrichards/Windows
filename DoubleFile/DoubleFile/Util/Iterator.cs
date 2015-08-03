using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DoubleFile
{
	internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
	{
		private int threadId;

		internal int state;

		internal TSource current;

		public TSource Current
		{
			get
			{
				return this.current;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		public Iterator()
		{
			this.threadId = Thread.CurrentThread.ManagedThreadId;
		}

		protected abstract Iterator<TSource> Clone();

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			this.current = default(TSource);
			this.state = -1;
		}

		public IEnumerator<TSource> GetEnumerator()
		{
			if (this.threadId == Thread.CurrentThread.ManagedThreadId && this.state == 0)
			{
				this.state = 1;
				return this;
			}
			Iterator<TSource> expr_29 = this.Clone();
			expr_29.state = 1;
			return expr_29;
		}

		public abstract bool MoveNext();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}
}
