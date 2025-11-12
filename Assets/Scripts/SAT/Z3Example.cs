using UnityEngine;
using Microsoft.Z3;

public class Z3Example : MonoBehaviour
{
    void Start()
    {
        // Contexto Z3: necessário para criar expressões e resolver
        using (var ctx = new Context())
        {
            // Variáveis booleanas
            BoolExpr a = ctx.MkBoolConst("a");
            BoolExpr b = ctx.MkBoolConst("b");

            // Fórmulas lógicas:
            // (a ∨ b) ∧ ¬a
            BoolExpr formula = ctx.MkAnd(ctx.MkOr(a, b), ctx.MkNot(a));

            // Criar o solver
            Solver s = ctx.MkSolver();
            s.Add(formula);

            // Checar satisfatibilidade
            Status result = s.Check();

            // Imprimir resultado no console da Unity
            Debug.Log("Resultado: " + result);

            // Se for SAT, imprime o modelo (valores de a e b)
            if (result == Status.SATISFIABLE)
            {
                Model m = s.Model;
                Debug.Log("Modelo: " + m);
            }
        }
    }
}
