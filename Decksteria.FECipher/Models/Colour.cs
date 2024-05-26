namespace Decksteria.FECipher.Models;

using System;

[Flags]
public enum Colour : uint
{
    Colourless = 0,
    Red = 1,
    Blue = 2,
    Yellow = 4,
    Purple = 8,
    Green = 16,
    Black = 32,
    White = 64,
    Brown = 128
}