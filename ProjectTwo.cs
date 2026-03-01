//============================================================================
// Name        : HashTable.cpp
// Author      : Lyn Conrad
// Version     : 1.0
// Copyright   : Copyright © 2023 SNHU COCE
// Description : Lab 4-2 Hash Table
//============================================================================

#include <algorithm>
#include <climits>
#include <iostream>
#include <string> // atoi
#include <time.h>
#include <vector>
#include <fstream>
#include <filesystem>
#include <cstdio>

// C++ 17
namespace fs = std::filesystem;
using namespace std;

//============================================================================
// Global definitions visible to all methods and classes
//============================================================================

const unsigned int DEFAULT_SIZE = 179;

// define a structure to hold course information
struct Course {
    string title;
    string courseNumber;
    vector<string>* prerequisites;

    // default initialization
    Course() {};

    // initialization with title and course number
    Course(string initialTitle, string initialNumber) : Course() {
        title = initialTitle;
        courseNumber = initialNumber;
    }
};

//============================================================================
// Hash Table class definition
//============================================================================

/**
 * Define a class containing data members and methods to
 * implement a hash table with chaining.
 */
class HashTable {

private:
    // Define structures to hold courses
    struct Node {
        Course course;
        unsigned int key;
        Node* next;

        // default constructor
        Node() {
            key = UINT_MAX;
            next = nullptr;
        }

        // initialize with a course
        Node(Course aCourse) : Node() {
            course = aCourse;
        }

        // initialize with a course and a key
        Node(Course aCourse, unsigned int aKey) : Node(aCourse) {
            key = aKey;
        }
    };

    vector<Node> nodes;

    unsigned int tableSize = DEFAULT_SIZE;

    unsigned int hash(int key);

public:
    HashTable();
    HashTable(unsigned int size);
    virtual ~HashTable();
    void Insert(Course course);
    void PrintInAlphanumericOrder();
    Course Search(string courseNumber);
    void PrintCourse(Course course);
};

/**
 * Default constructor
 */
HashTable::HashTable() {
    // Initalize node structure by resizing tableSize
    nodes.resize(tableSize);
}

/**
 * Constructor for specifying size of the table
 * Use to improve efficiency of hashing algorithm
 * by reducing collisions without wasting memory.
 */
HashTable::HashTable(unsigned int size) {
    // Resize tablesize and nodes size
    this->tableSize = size;
    nodes.resize(tableSize);
}


/**
 * Destructor
 */
HashTable::~HashTable() {
    // Erase nodes beginning
    nodes.erase(nodes.begin());
}

/**
 * Calculate the hash value of a given key.
 * Note that key is specifically defined as
 * unsigned int to prevent undefined results
 * of a negative list index.
 *
 * @param key The key to hash
 * @return The calculated hash
 */
unsigned int HashTable::hash(int key) {
    // Calculate hash value
    return key % tableSize;
}

/**
 * Insert a course
 *
 * @param course The course to insert
 */
void HashTable::Insert(Course course) {
    // Create the key for the given course
    unsigned key = hash(atoi(course.courseNumber.c_str()));

    // Retrieve the node using the key
    Node* oldNode = &(nodes.at(key));

    // If no entry found for this key
    if (oldNode == nullptr) {
        // Assign this node to the key position
        Node* newNode = new Node(course, key);
        nodes.insert(nodes.begin() + key, (*newNode));
    }
    // Else node found
    else
    {
        if (oldNode->key == UINT_MAX) {
            // Assign old node key to UINT_MAX
            oldNode->key = key;
            // Set old node to course
            oldNode->course = course;
            // Set old node next to null pointer
            oldNode->next = nullptr;
        }
        // else find the next open node
        else {
            // find the next open node (last one)
            while (oldNode->next != nullptr) {
                oldNode = oldNode->next;
            }

            // add new newNode to end
            oldNode->next = new Node(course, key);
        }
    }
}

/**
 * Print all courses in alphanumeric order
 */
void HashTable::PrintInAlphanumericOrder() {

    // create a copy of the nodes vector for sorting
    vector<Node> orderedList = nodes;

    // sort in alphanumeric order using the algorithm sort method
    // https://www.geeksforgeeks.org/cpp/sorting-a-vector-in-c/
    sort(
        orderedList.begin(), orderedList.end(),
        [](const Node& left, const Node& right)
        { return left.course.courseNumber < right.course.courseNumber; }
    );

    // for node begin to end iterate
    for (unsigned int i = 0; i < orderedList.size(); ++i) {
        PrintCourse(orderedList.at(i).course);
    }
}

/**
 * Search for the specified courseNumber
 *
 * @param courseNumber The course number to search for
 */
Course HashTable::Search(string courseNumber) {
    Course course;

    // Create the key for the given course
    unsigned key = hash(atoi(course.courseNumber.c_str()));

    Node* node = &(nodes.at(key));

    // If node found that matches key
    if (node != nullptr && node->key != UINT_MAX && node->course.courseNumber.compare(courseNumber) == 0)
    {
        // return node course
        return node->course;
    }

    // If no entry found
    if (node == nullptr || node->key == UINT_MAX)
    {
        // return course
        return course;
    }

    // While node not equal to nullptr
    while (node != nullptr) {
        // If the current node matches, return it
        if (node->key != UINT_MAX && node->course.courseNumber.compare(courseNumber) == 0) {
            return node->course;
        }
        // Node is equal to next node
        node = node->next;
    }

    return course;
}

void HashTable::PrintCourse(Course course) {
    cout << "Course title: " << course.title << endl;
    cout << "Course number: " << course.courseNumber << endl;
    cout << "Prerequisites: ";

    for (unsigned int j = 0; j < course.prerequisites->size(); ++j) {
        cout << course.prerequisites->at(j);
    }
}


/**
 * The one and only main() method
 */
int main(int argc, char* argv[]) {

    // Define a hash table to hold all the bids
    HashTable* courseTable;
    ifstream coursesFS;
    fs::path filePath{ "ABCU_Advising_Program_Input.csv" };

    // temp variables
    string line;
    vector<string> parameters;
    string parameter;
    Course existingCourse;

    Course course;
    courseTable = new HashTable();

    int choice = 0;
    while (choice != 9) {
        cout << "Menu:" << endl;
        cout << "  1. Load file data" << endl;
        cout << "  2. Print courses" << endl;
        cout << "  3. Find Course" << endl;
        cout << "  4. Exit" << endl;
        cout << "Enter choice: ";
        cin >> choice;

        switch (choice) {

        case 1:
            // open the file
            coursesFS.open("courses.txt");

            // check if file is open
            if (!coursesFS.is_open()) {
                throw new exception("Failed to open file");
            }

            // check if file is in Comma Separated Values (CSV) format
            if (filePath.extension() != "csv") {
                throw new exception("Incorrect file format detected");
            }

            // loop until failure encountered or end of text file
            while (!coursesFS.fail() && coursesFS.eof()) {

                // collect number of parameters in the line
                while (getline(coursesFS, parameter, ',')) {
                    parameters.push_back(parameter);
                }

                // if number of parameters per line is less than two, throw an exception
                if (parameters.size() < 2) {
                    throw new exception("Incorrect numbers of parameters per line");
                }

                // create the course using the course title and number
                course = Course(parameters.at(1), parameters.at(0));

                // for each of the remainding parameters (prerequisites)
                for (int i = 0; (parameters.size() - 2) < i; ++i) {

                    // try to find the course in the courses list
                    existingCourse = courseTable->Search(parameters.at(i));

                    // if the prerequisite doesn't exist as a course, throw an error
                    if (existingCourse.courseNumber.empty()) {
                        throw new exception("Unable to find prerequisite");
                    }
                    // otherwise, add the prerequisite to the list
                    else {
                        course.prerequisites->push_back(parameters.at(i));
                    }
                }

                // add course to vector of courses
                courseTable->Insert(course);
            }

            // catch failure before end of text file
            if (!coursesFS.eof()) {
                throw new exception("Failed to read courses from file.");
            }

            break;

        case 2:
            courseTable->PrintInAlphanumericOrder();
            break;

        case 3:
            course = courseTable->Search("CSCI100");

            if (!course.courseNumber.empty()) {
                courseTable->PrintCourse(course);
            }
            else {
                cout << "Course Id " << "CSCI100" << " not found." << endl;
            }

            break;
        }
    }

    cout << "Good bye." << endl;

    return 0;
}


