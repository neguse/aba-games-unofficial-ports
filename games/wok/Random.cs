public static class WkRandom {
 static int[] state=WkArrays.Make(31,()=>0);
 static int front=3,rear;
 public static void Seed(int seed){
  if(seed<=0)seed=1;state[0]=seed;
  for(int i=1;i<31;i++){int next=16807*(seed%127773)-2836*(seed/127773);if(next<0)next+=2147483647;seed=next;state[i]=next;}
  front=3;rear=0;for(int i=0;i<310;i++)rand();
 }
 public static int rand(){int sum=state[front]+state[rear];state[front]=sum;front=(front+1)%31;rear=(rear+1)%31;return (sum>>1)&2147483647;}
 public static int randN(int n){return n>0?rand()%n:0;}
 public static int randNS(int n){bool positive=(rand()&1)!=0;int value=randN(n);return positive?value:-value;}
}
