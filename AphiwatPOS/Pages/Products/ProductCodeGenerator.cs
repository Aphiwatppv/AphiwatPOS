namespace AphiwatPOS.Pages.Products;

internal static class ProductCodeGenerator
{
    public static async Task<string> NextAsync(
        string prefix,
        int digitCount,
        Func<string, Task<bool>> codeExistsAsync)
    {
        for (var sequence = 0; sequence <= 99999; sequence++)
        {
            var code = $"{prefix}-{sequence.ToString(new string('0', digitCount))}";
            if (!await codeExistsAsync(code))
            {
                return code;
            }
        }

        throw new InvalidOperationException($"Unable to generate the next {prefix} code.");
    }
}
