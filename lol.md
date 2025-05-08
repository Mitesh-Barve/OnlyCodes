🔹 What is a CNN?
CNN stands for Convolutional Neural Network, a deep learning model mainly used for image classification and recognition tasks.

It automatically learns features from images through multiple layers.

🔹 Key Components of CNN Architecture
Input Layer: Takes the image input (e.g., 28x28 pixels).

Convolutional Layers: Apply filters (kernels) to detect features like edges, shapes, etc.

Activation Function (e.g., ReLU): Introduces non-linearity.

Pooling Layers: Downsample the feature maps (e.g., max pooling), reducing size and computation.

Dropout Layer (optional): Prevents overfitting by randomly deactivating neurons during training.

Fully Connected Layers (Dense): Flatten the data and classify into categories.

Softmax Layer: Outputs a probability distribution across classes.

Loss Function (e.g., Cross-Entropy): Measures the difference between prediction and actual label.

Optimizer (e.g., SGD, Adam): Updates weights to minimize the loss function.

🔹 How CNNs Perform Classification
Input image → feature extraction → classification.

Training involves feeding labeled images to learn the best filters and weights.

Prediction uses the trained model to classify new images.

🔹 Applications of CNNs
Image Classification

Object Detection

Semantic Segmentation

Natural Language Processing (e.g., sentiment analysis)

Medical Imaging (e.g., tumor detection)

Autonomous Vehicles

Video Analysis

🔹 Fashion MNIST Dataset
A dataset of 70,000 grayscale images (28x28) across 10 clothing categories.

Training: 60,000 images | Testing: 10,000 images.

Categories: T-shirts/tops, trousers, pullovers, dresses, coats, sandals, shirts, sneakers, bags, ankle boots.

More challenging than the original digit-based MNIST.

🔹 Steps to Implement CNN on Fashion MNIST
Import libraries: TensorFlow, Keras, NumPy, etc.

Load dataset: fashion_mnist.load_data().

Preprocess data: Normalize pixel values, reshape to (28, 28, 1).

Define CNN architecture: Conv layers, pooling, activation functions, etc.

Compile model: Choose loss, optimizer (e.g., Adam), and metrics.

Train model: Use .fit() with epochs and batch size.

Evaluate model: Use .evaluate() to measure accuracy and loss.

Predict new data: Use .predict() on unseen images.

