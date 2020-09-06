using System;
using System.Runtime.Serialization;

namespace SqlBuilderFramework
{
    /// <summary>
    /// Wird benutzt wenn bei einem SqlDataReader eine Spalte nicht gefunden wird
    /// </summary>
    [Serializable]
    public class ColumnNotFoundException : Exception
    {
        public string ColumnName { get; private set; }

        public ColumnNotFoundException(string name)
        {
            ColumnName = name;
        }

        /// <summary>
        /// Setzt im Serialisationsinfo die Properties dieser Klasse.
        /// Wird für die Serialisierung benötigt. 
        /// </summary>
        /// <param name="info">Das Serialisationsinfo.</param>
        /// <param name="context">Der Streamingkontext.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ColumnName", ColumnName);
        }
    }
}
