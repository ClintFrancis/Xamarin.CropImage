# Xamarin.CropImage

This is a Xamarin.iOS binding for the TOCropViewController library from Tim Oliver.

TOCropViewController is an open-source UIViewController subclass built to allow users to perform basic manipulation on UIImage objects; specifically cropping and some basic rotations. It has been designed with the iOS 8 Photos app in mind, and as such, behaves in an already familiar way.

## Features

- Crop images by dragging the edges of a grid overlay.
- Optionally, crop circular copies of images.
- Rotate images in 90-degree segments.
- Clamp the crop box to a specific aspect ratio.
- A reset button to completely undo all changes.
- iOS 7/8 translucency to make it easier to view the cropped region.
- The choice of having the controller return the cropped image to a delegate, or immediately pass it to a UIActivityViewController.
- A custom animation and layout when the device is rotated to landscape mode.
- Custom 'opening' and 'dismissal' animations.
- Localized in 18 languages.

## Credits

The original Objective C library was created by Tim Oliver
[https://github.com/TimOliver/TOCropViewController](https://github.com/TimOliver/TOCropViewController)
