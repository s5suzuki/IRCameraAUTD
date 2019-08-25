using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PI450Viewer
{
    public static class PropertyName<TInstance>
    {
        public static string Get<TMember>(Expression<Func<TInstance, TMember>> propertyExpression)
        {
            if (propertyExpression?.Body is MemberExpression memberExp)
            {
                return memberExp.Member.Name;
            }

            throw new ArgumentException("Can't cast to MemberExpression", nameof(propertyExpression));
        }
    }

    public static partial class PropertyChangedEventHandleExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, params string[] propertyNames)
        {
            if (handler == null || propertyNames == null)
            {
                return;
            }

            foreach (string name in propertyNames)
            {
                handler(sender, new PropertyChangedEventArgs(name));
            }
        }
    }
}