namespace Decksteria.FECipher.Services;

using Decksteria.FECipher.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

internal interface IFECardListService
{
    Task<IEnumerable<FECard>> GetCardList();
}