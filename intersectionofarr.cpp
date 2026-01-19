# how to comine intersection of 2 arrays 
class Solution {
public:
    vector<int> intersect(vector<int>& nums1, vector<int>& nums2) {
        sort(nums1.begin(), nums1.end());
        sort(nums2.begin(), nums2.end());

        int i = 0, j = 0;
        vector<int> ans;

        while (i < nums1.size() && j < nums2.size()) {
            if (nums1[i] < nums2[j]) {
                i++;
            }
            else if (nums1[i] > nums2[j]) {
                j++;
            }
            else {
                ans.push_back(nums1[i]);
                i++;
                j++;
            }
        }
        return ans;
    }
};
********************************************************************************************************************************************************
#by freq 
vector<int> intersect(vector<int>& nums1, vector<int>& nums2) {
    unordered_map<int, int> freq;
    vector<int> ans;

    for (int x : nums1)
        freq[x]++;

    for (int x : nums2) {
        if (freq[x] > 0) {
            ans.push_back(x);
            freq[x]--;
        }
    }
    return ans;
}

