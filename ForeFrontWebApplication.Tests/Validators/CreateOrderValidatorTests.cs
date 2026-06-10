using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ForeFrontWebApplication.Tests.Validators
{
    public class CreateOrderValidatorTests
    {
        private readonly CreateOrderValidator _sut = new();

        [Fact]
        public void ShouldFailWhenNoLines()
        {
            var cmd = new CreateOrderCommand
            {
                KundId = "customer-1",
                Produkter = []
            };

            var result = _sut.Validate(cmd);
        }
    }
}
