using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GNFSPoly
{

    /// <summary>
    /// Enables using a property expression to read property names and set property values
    /// </summary>
    public partial class GnfsPolyReader
    {
        public class PropertyHelper
        {

            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> of the target <paramref name="propExpression"/>.
            /// </summary>
            /// <typeparam name="TDeclaring">The type declaring the property</typeparam>
            /// <typeparam name="TProp">The target property</typeparam>
            /// <param name="propExpression">The expression referencing the target property,
            ///  For example: 'x=> targetInstance.TargetProperty </param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static PropertyInfo
                GetProperty<TDeclaring, TProp>
                (
                    Expression<Func<TDeclaring, TProp>> propExpression
                )
            {
                if (propExpression == null)
                    throw new ArgumentNullException("getter");

                var expr = propExpression.Body as MemberExpression;

                if (expr == null)
                    throw new ArgumentException("Body must be a member expression.", nameof(propExpression));

                var prop = expr.Member as PropertyInfo;

                if (prop == null)
                    throw new ArgumentException("Expression target must be a property", nameof(propExpression));


                return prop;

            }

            /// <summary>
            /// Gets the <see cref="MethodInfo.Name"/> of the target <paramref name="propExpression"/>.
            /// </summary>
            /// <typeparam name="TDeclaring">The type declaring the property</typeparam>
            /// <typeparam name="TProp">The target property</typeparam>
            /// <param name="propExpression">The expression referencing the target property,
            ///  For example: 'x=> targetInstance.TargetProperty </param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static string
                GetPropertyName<TDeclaring, TProp>
                (
                    Expression<Func<TDeclaring, TProp>> propExpression
                )
                    => GetProperty(propExpression).Name;


            /// <summary>
            /// Creates setter action for the target for the target <paramref name="propExpression"/>.
            /// </summary>
            /// <typeparam name="TDeclaring">The type declaring the property</typeparam>
            /// <typeparam name="TProp">The target property</typeparam>
            /// <param name="propExpression">The expression referencing the target property,
            ///  For example: 'x=> targetInstance.TargetProperty </param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static Action<TDeclaring, TProp>
                CreateSetter<TDeclaring, TProp>
                (
                    Expression<Func<TDeclaring, TProp>> propExpression
                )
            {
                var prop = GetProperty(propExpression);

                if (!prop.CanWrite)
                    throw new ArgumentException($"Target property {prop.Name} must be writeable", nameof(propExpression));

                return CreateSetAction<TDeclaring, TProp>(prop.GetSetMethod());
           

            }

            /// <summary>
            /// Creates a strongly type generic action for the target <paramref name="method"/>.
            /// </summary>
            /// <typeparam name="TDeclaring">The type declaring the property</typeparam>
            /// <typeparam name="TProp">The target property</typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static Action<TDeclaring, TProp>
                CreateSetAction<TDeclaring, TProp>
                (
                    MethodInfo method
                )
            {
                var del = Delegate
                    .CreateDelegate(
                        typeof(Action<TDeclaring, TProp>),
                        method
                        );
                return (Action<TDeclaring, TProp>)del;
            }
        }
    }
}
