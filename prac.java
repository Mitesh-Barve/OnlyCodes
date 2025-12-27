public class Main {
    public static void main(String[] args) {
        String s = "java";
        String rev = "";
        int count = 0 ;
              
            for(int i=s.length()-1 ; i>= 0 ; i--){
                rev = rev + s.charAt(i);
            }

        for(int i = 0 ; i <= s.length()-1 ; i++){
             char ch = s.charAt(i) ;
            if(ch == 'a' || ch == 'e' || ch == 'i' || ch == 'o' || ch == 'u')
                count++;
        }
        
        System.out.println(rev);
        System.out.println(count);
    }
/**********************************************************************************************************/
import java.util.Scanner;

public class Main {
    public static void main(String[] args) {

        Scanner sc = new Scanner(System.in);

        int n = sc.nextInt();
        int[] arr = new int[n];

        for (int i = 0; i < n; i++) {
            arr[i] = sc.nextInt();
        }

        int sum = 0;
        for (int i = 0; i < n; i++) {
            sum += arr[i];
        }

        for (int i = 0; i < n; i++
