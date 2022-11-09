#import <WebKit/WebKit.h>

@interface WebViewController : UIViewController <UIWebViewDelegate>

// MARK: - Properties

@property(nonatomic) WKWebView* webView;
@property(nonatomic) NSURL* url;


// MARK: - Setup

-(void)setupWebView;


// MARK: - Configure

-(void)configureWithURL:(NSURL*)url;

@end
