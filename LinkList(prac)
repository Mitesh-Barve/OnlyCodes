// Online C++ compiler to run C++ program online
#include <iostream>
using namespace std;

class Node{
    public:
       int data;
       Node *next;
       
       Node(int val){
           data = val;
           next = NULL;
       }
};
int main() {
     Node *Head;
     Head=NULL;
     int arr[] = {22,43,67 ,89};
   
   //Insert the node at beginning
   //LL doesnt exist 
    for(int i=0;i<4;i++){
      if(Head==NULL){
        Head = new Node(arr[i]);
      }
   //LL does exist 
      else{
       Node *temp;
       temp=new Node(arr[i]);
       temp->next=Head ;
       Head=temp;
      }
      
    }
    
    //Print the value 
    Node *temp = Head ;
    while(temp){
        cout<<temp->data<<" ";
        temp = temp->next;
    }
   
    return 0;
}
