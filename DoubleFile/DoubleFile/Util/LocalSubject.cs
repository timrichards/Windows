using System;
using System.Reactive.Subjects;

namespace DoubleFile
{
    public class LocalSubject<T> : ISubject<Tuple<T, int>>
    {
        public void OnCompleted() => _subject.OnCompleted();
        public void OnError(Exception e) => Util.Assert(99772, false, e.GetBaseException().GetType() + " in LocalSubject\n" + e.GetBaseException().Message + "\n" + e.StackTrace);
        public void OnNext(Tuple<T, int> value) => _subject.OnNext(value);
        public IDisposable Subscribe(IObserver<Tuple<T, int>> observer) => _subject.Subscribe(observer);

        Subject<Tuple<T, int>>
            _subject = new Subject<Tuple<T, int>>();
    }
}
