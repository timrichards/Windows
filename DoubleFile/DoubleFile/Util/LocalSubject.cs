using System;
using System.Reactive.Subjects;

namespace DoubleFile
{
    class LocalSubject<T> : ISubject<Tuple<T, int>>
    {
        public void OnCompleted() { _subject.OnCompleted(); }
        public void OnError(Exception error) { _subject.OnError(error); }
        public void OnNext(Tuple<T, int> value) { _subject.OnNext(value); }
        public IDisposable Subscribe(IObserver<Tuple<T, int>> observer) { return _subject.Subscribe(observer); }

        Subject<Tuple<T, int>>
            _subject = new Subject<Tuple<T, int>>();
    }
}
