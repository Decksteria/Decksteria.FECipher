﻿namespace Decksteria.FECipher.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.FECipher.Models;

internal interface IFECardListService
{
    public Task<IEnumerable<FECard>> GetCardList(CancellationToken cancellationToken = default);
}