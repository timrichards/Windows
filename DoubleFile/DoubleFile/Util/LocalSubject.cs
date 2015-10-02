using System;
using System.Reactive.Subjects;

namespace DoubleFile
{
    public class LocalSubject<T> : ISubject<Tuple<T, decimal>>
    {
        public void OnCompleted() => _subject.OnCompleted();
        public void OnError(Exception e) => Util.Assert(99772, false, e.GetBaseException().GetType() + " in LocalSubject\n" + e.GetBaseException().Message + "\n" + e.StackTrace);
        public void OnNext(Tuple<T, decimal> value) => _subject.OnNext(value);
        public IDisposable Subscribe(IObserver<Tuple<T, decimal>> observer) => _subject.Subscribe(observer);

        ISubject<Tuple<T, decimal>>
            _subject = new Subject<Tuple<T, decimal>>();
    }
}
