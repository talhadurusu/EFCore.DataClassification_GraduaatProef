using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.DataClassification.Exceptions {

    /// <summary>
    /// EFCore.DataClassification library'sinde
    /// modeldeki yanlış / tutarsız DataClassification
    /// konfigurasyonlarını temsil eden exception.
    /// </summary>
    public sealed class DataClassificationException : InvalidOperationException {
        public IProperty? Property { get; }

        public DataClassificationException() {
        }

        public DataClassificationException(string message)
            : base(message) {
        }

        public DataClassificationException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public DataClassificationException(IProperty property, string message)
            : base(message) {
            Property = property;
        }

        public DataClassificationException(IProperty property, string message, Exception innerException)
            : base(message, innerException) {
            Property = property;
        }
    }
}