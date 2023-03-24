using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SignPadPicker
{
    public abstract class NotifyPropertyImpl : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (property.Body is MemberExpression body)
                OnPropertyChanged(body.Member.Name);
            else
                throw new ArgumentException("The body must be a member expression");
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void OnPropertyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
}
