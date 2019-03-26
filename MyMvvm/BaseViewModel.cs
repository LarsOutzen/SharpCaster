using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyMvvm
{
    public class BaseViewModel : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePC<T>(Expression<Func<T>> propertyExpression)
        {
            if(propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpr.Member.Name;
                this.OnChanged(propertyName);
            }
        }
     
        //protected virtual string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        //{
        //    string propName = null;

        //    if(propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
        //    {
        //        MemberExpression mbrExp = propertyExpression.Body as MemberExpression;
        //        propName = mbrExp.Member.Name;
        //    }
        //    return propName;
        //}

        public virtual bool RaisePcIfChanged<T>(Expression<Func<T>> propFieldExpression, T newValue,
                                                Expression<Func<T>> propExpression, object propertyFieldOwner = null)
        {
            bool retVal = false;
            if(propFieldExpression.Body.NodeType == ExpressionType.MemberAccess && propExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                object PropertyOwner = propertyFieldOwner == null ? this : propertyFieldOwner;
                // TypedReference tr = __makeref(PropertyOwner);

                MemberExpression MemberField = propFieldExpression.Body as MemberExpression;
                MemberExpression MemberProperty = propExpression.Body as MemberExpression;

                FieldInfo fieldInfo = PropertyOwner.GetType().GetField(MemberField.Member.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if(fieldInfo != null)
                {
                    object fieldValue = fieldInfo.GetValue(PropertyOwner);

                    if(((fieldValue == null) && (newValue != null)) || ((fieldValue != null) && !fieldValue.Equals(newValue)))
                    {
                        fieldInfo.SetValue(PropertyOwner, newValue);
                        retVal = true;
                        OnChanged(MemberProperty.Member.Name);
                    }
                }
                else
                {
                    PropertyInfo prpInfo = PropertyOwner.GetType().GetProperty(MemberField.Member.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    object prpValue = prpInfo.GetValue(PropertyOwner, null);
                    if(((prpValue == null) && (newValue != null)) || ((prpValue != null) && !prpValue.Equals(newValue)))
                    {
                        prpInfo.SetValue(PropertyOwner, newValue, null);
                        retVal = true;
                        OnChanged(MemberProperty.Member.Name);
                    }
                }
            }
            return retVal;
        }
    }
}
