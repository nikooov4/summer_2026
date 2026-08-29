using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.Expression1
{
    public class Expression1<TDomain,TCodomain>
    {
        public Expression1<TCodomain,TCodomain2> Composition<TCodomain2>(Expression1<TDomain,TCodomain> expr)
        {
            throw new NotImplementedException();
        }
    }

    public class PredicateExpression<TDomain> : Expression1<TDomain,bool>
    {
        public PredicateExpression<TDomain> And(PredicateExpression<TDomain> expr)
        {
            throw new NotImplementedException();
        }

        public PredicateExpression<TDomain> Or(PredicateExpression<TDomain> expr)
        {
            throw new NotImplementedException();
        }

        public PredicateExpression<TDomain> Negate()
        {
            throw new NotImplementedException();
        }

    }
}
