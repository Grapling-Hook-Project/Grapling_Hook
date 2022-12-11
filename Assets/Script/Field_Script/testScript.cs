using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{
    [SerializeField] float move1 = 0;
    [SerializeField] float move2 = 0;
    [SerializeField] float move3 = 0;
    int counter = 0;
    void Update()
    {
        Vector3 p = new Vector3(move1, move2, move3);
        transform.Translate(p);
        counter++;
        test(move1, move2, move3, counter);
        //count‚ª100‚É‚È‚ê‚Î-1‚ðŠ|‚¯‚Ä‹t•ûŒü‚É“®‚©‚·
    }

    void test(float move1_, float move2_, float move3_, int a)
    {
        if (counter == 50)
        {
            a = 0;
            move1_ *= -1;
            move2_ *= -1;
            move3_ *= -1;
        }
    }
}
