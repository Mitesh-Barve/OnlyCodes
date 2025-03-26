//**************** character problems **********************/
#include <iostream>
using namespace std;
int main (){
    //  int n;
    // cin>>n;
    char c = ' a ';
     for(int i=1 ; i<= 5 ; i++){
           char name = 'a' + (i -1) ;
         for(int j=1; j <= 5 ; j++){
             cout<<name<< " ";
         }
         cout<<endl;
     }
    return 0;
}
OP:-
a a a a a 
b b b b b 
c c c c c 
d d d d d 
e e e e e 
//*************************************************************/
#include <iostream>
using namespace std;

int main() {
    for (int i = 1; i <= 5; i++) {   
        for (char c = 'a'; c <= 'e'; c++) {  
            cout << c << " ";
        }
        cout << endl;
    }
    return 0;
}
OP:-
a b c d e 
a b c d e 
a b c d e 
a b c d e 
a b c d e 

