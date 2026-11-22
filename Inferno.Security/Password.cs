using System.Security.Cryptography;

namespace Inferno.Security;

// http://stackoverflow.com/questions/38995379/alternative-to-system-web-security-membership-generatepassword-in-aspnetcore-ne
/// <summary>Builds a random password from digits, letters, and a fixed set of punctuation.</summary>
public static class Password
{
    private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

    /// <summary>
    /// Builds a password of <paramref name="length"/> characters.
    /// The first pass uses a cryptographic generator. When that pass has fewer punctuation characters than requested, later positions are replaced using <see cref="Random"/>.
    /// </summary>
    /// <param name="length">How many characters to return. Must be from 1 to 128.</param>
    /// <param name="numberOfNonAlphanumericCharacters">How many punctuation characters the result must contain. Must be from 0 to <paramref name="length"/>.</param>
    /// <returns>The generated password.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="length"/> is outside 1 to 128, or <paramref name="numberOfNonAlphanumericCharacters"/> is negative or greater than <paramref name="length"/>.
    /// </exception>
    public static string Generate(int length, int numberOfNonAlphanumericCharacters)
    {
        if (length is < 1 or > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfNonAlphanumericCharacters));
        }

        using var rng = RandomNumberGenerator.Create();
        byte[] byteBuffer = new byte[length];

        rng.GetBytes(byteBuffer);

        int count = 0;
        char[] characterBuffer = new char[length];

        for (int iter = 0; iter < length; iter++)
        {
            int i = byteBuffer[iter] % 87;

            if (i < 10)
            {
                characterBuffer[iter] = (char)('0' + i);
            }
            else if (i < 36)
            {
                characterBuffer[iter] = (char)('A' + i - 10);
            }
            else if (i < 62)
            {
                characterBuffer[iter] = (char)('a' + i - 36);
            }
            else
            {
                characterBuffer[iter] = Punctuations[i - 62];
                count++;
            }
        }

        if (count >= numberOfNonAlphanumericCharacters)
        {
            return new string(characterBuffer);
        }

        int j;
        var rand = new Random();

        for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
        {
            int k;
            do
            {
                k = rand.Next(0, length);
            }
            while (!char.IsLetterOrDigit(characterBuffer[k]));

            characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
        }

        return new string(characterBuffer);
    }
}
