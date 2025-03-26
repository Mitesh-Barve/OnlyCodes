//Prime or not //
#include<iostream>
using namespace std;

int main() {
    int n, i;
    cout << "Enter a number: ";
    cin >> n;

    // If the number is less than 2, it's not prime
    if (n < 2) {
        cout << "Not a prime number";
        return 0;
    }
    else {
        // Check for factors of n from 2 to n-1
        for (i = 2; i < n; i++) {
            if (n % i == 0) {
                cout << "Not a prime number";
                return 0;
            }
        }
    }

    // If no factors were found, it's a prime number
    cout << "Prime number";
    return 0;
}
//****************************IN CERTAIN RANGE***************************************//
#include <iostream>
#include <cmath>  // For sqrt function
using namespace std;

// Function to check if a number is prime
bool isPrime(int n) {
    if (n < 2) return false;
    for (int i = 2;  i < n; i++) {
        if (n % i == 0) return false;
    }
    return true;
}

int main() {
    int low, high;

    cout << "Enter two numbers (intervals): ";
    cin >> low >> high;

    cout << "Prime numbers between " << low << " and " << high << " are: ";
    for (int i = low; i <= high; i++) {
        if (isPrime(i)) {
            cout << i << " ";
        }
    }
    cout << endl;
    
    return 0;
}
