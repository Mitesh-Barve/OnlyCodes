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
*********************************************************************************************************XD****************************************************************************************
    //USING RECURSION//
    #include <iostream>
using namespace std;

class Node {
public:
    int data;
    Node* next;

    Node(int value) {
        data = value;
        next = NULL;
    }
};

Node* createLinkedList(int arr[], int index, int size) {
    // BASE CASE
    if (index == size)
        return NULL;

    Node* temp = new Node(arr[index]);
    temp->next = createLinkedList(arr, index + 1, size);

    return temp;
}

int main() {

    Node* Head = NULL;
    int arr[] = {2, 4, 6, 8, 10};
    Head = createLinkedList(arr, 0, 5);

    Node* temp = Head;
    while (temp) {
        cout << temp->data << " ";
        temp = temp->next;
    }

    // Delete dynamically allocated memory to prevent memory leaks
    temp = Head;
    while (temp) {
        Node* next = temp->next;
        delete temp;
        temp = next;
    }

    return 0;
}

