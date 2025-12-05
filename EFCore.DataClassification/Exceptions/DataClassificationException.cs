using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.DataClassification.Exceptions {

    /// <summary>
    /// Exception that represents invalid or inconsistent
    /// DataClassification configurations in the EF Core model.
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